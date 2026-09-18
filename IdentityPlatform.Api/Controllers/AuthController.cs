using IdentityPlatform.Identity.Application.MFA.ChangeMfaType;
using IdentityPlatform.Identity.Application.MFA.SetupMfa;
using IdentityPlatform.Identity.Application.MFA.VerifyAndEnableMfa;
using IdentityPlatform.Identity.Application.Users.Commands.ChangePassword;
using IdentityPlatform.Identity.Application.Users.Commands.ForgotPassword;
using IdentityPlatform.Identity.Application.Users.Commands.Login;
using IdentityPlatform.Identity.Application.Users.Commands.Logout;
using IdentityPlatform.Identity.Application.Users.Commands.RefreshToken;
using IdentityPlatform.Identity.Application.Users.Commands.RegisterUser;
using IdentityPlatform.Identity.Application.Users.Commands.ResetPassword;
using IdentityPlatform.Identity.Application.Users.Commands.RevokeAllSessions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Claims;

namespace IdentityPlatform.Api.Controllers
{
    [Route("api/[controller]")]
    [EnableRateLimiting("FixedGlobal")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ISender _sender;

        public AuthController(ISender sender)
        {
            _sender = sender;
        }

        /// <summary>
        /// تسجيل مستخدم جديد
        /// </summary>
        [HttpPost("register")]
        public async Task<IActionResult> Register(
            [FromBody] RegisterUserCommand command,
            CancellationToken cancellationToken)
        {
            var result = await _sender.Send(command, cancellationToken);

            // التحقق من نتيجة الـ Result Pattern
            if (result.IsFailure)
            {
                return BadRequest(new
                {
                    Code = result.Error.GetHashCode(),
                    Message = result.Error.ErrorMessage
                });
            }

            return Ok(new { UserId = result.Value });
        }

        /// <summary>
        /// المرحلة الأولى: التحقق من بيانات الدخول وبدء تحدي الـ MFA الإلزامي
        /// </summary>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginCommand command, CancellationToken cancellationToken)
        {
            var result = await _sender.Send(command, cancellationToken);

            if (result.IsFailure)
            {
                return Unauthorized(new { error = result.Error.ErrorMessage });
            }

            return Ok(result.Value);
        }

        /// <summary>
        /// المرحلة الثانية: التحقق من رمز الـ MFA (البريد أو TOTP) وإصدار التوكنات النهائية
        /// </summary>
        [HttpPost("verify-mfa")]
        public async Task<IActionResult> VerifyMfa([FromBody] VerifyLoginMfaCommand command, CancellationToken cancellationToken)
        {
            var result = await _sender.Send(command, cancellationToken);

            if (result.IsFailure)
            {
                return BadRequest(new { error = result.Error.ErrorMessage });
            }

            return Ok(result.Value);
        }

        /// <label>تجديد التوكن (Token Rotation)</label>
        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenCommand command, CancellationToken cancellationToken)
        {
            var result = await _sender.Send(command, cancellationToken);

            if (result.IsFailure)
            {
                return BadRequest(result.Error);
            }

            return Ok(result.Value);
        }

        /// <label>طلب استعادة كلمة المرور (إرسال الرمز)</label>
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordCommand command, CancellationToken cancellationToken)
        {
            var result = await _sender.Send(command, cancellationToken);

            if (result.IsFailure)
            {
                return BadRequest(result.Error);
            }

            return Ok(result.Value);
        }

        /// <label>إعادة تعيين كلمة المرور باستخدام الرمز</label>
        [HttpPost("reset-password")]
        [EnableRateLimiting("StrictAuth")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordCommand command, CancellationToken cancellationToken)
        {
            var result = await _sender.Send(command, cancellationToken);

            if (result.IsFailure)
            {
                return BadRequest(result.Error);
            }

            return Ok(result.Value);
        }

        /// <label>تسجيل الخروج (إبطال عائلة التوكنات)</label>
        [HttpPost("logout")]
        public async Task<IActionResult> Logout([FromBody] LogoutCommand command, CancellationToken cancellationToken)
        {
            var result = await _sender.Send(command, cancellationToken);

            if (result.IsFailure)
            {
                return BadRequest(result.Error);
            }

            return Ok(result.Value);
        }

        /// <summary>
        /// تغيير كلمة المرور للمستخدم الحالي
        /// </summary>
        [Authorize] // تتطلب تسجيل الدخول
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequestDto request, CancellationToken cancellationToken)
        {
            // استخراج معرف المستخدم الحالي من الـ Claims (JWT Token)
            if (!TryGetUserId(out var userId))
            {
                return Unauthorized();
            }

            var command = new ChangePasswordCommand(userId, request.CurrentPassword, request.NewPassword);

            var result = await _sender.Send(command, cancellationToken);

            if (result.IsFailure)
            {
                // يمكنك تحويل خطأ النطاق (Domain Error) إلى استجابة مناسبة HTTP 400 أو غيرها
                return BadRequest(result.Error);
            }

            return Ok(new { message = "تم تغيير كلمة المرور وإبطال الجلسات السابقة بنجاح." });
        }

        public sealed record ChangePasswordRequestDto(
    string CurrentPassword,
    string NewPassword
);

        /// <summary>
        /// إلغاء جميع جلسات المستخدم من كافة الأجهزة (Revoke All Sessions)
        /// </summary>
        [Authorize] // تتطلب تسجيل الدخول
        [HttpPost("revoke-all-sessions")]
        public async Task<IActionResult> RevokeAllSessions(CancellationToken cancellationToken)
        {
            // استخراج معرف المستخدم الحالي من الـ Claims
            if (!TryGetUserId(out var userId))
            {
                return Unauthorized();
            }

            var command = new RevokeAllSessionsCommand(userId);

            var result = await _sender.Send(command, cancellationToken);

            if (result.IsFailure)
            {
                return BadRequest(result.Error);
            }

            return Ok(new { message = "تم تسجيل الخروج من جميع الأجهزة وإلغاء كافة الجلسات بنجاح." });
        }

        // دالة مساعدة لاستخراج الـ UserId من الـ User Claims
        private bool TryGetUserId(out Guid userId)
        {
            userId = Guid.Empty;
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            return Guid.TryParse(userIdClaim, out userId);
        }

        /// <summary>
        /// خطوة إعداد الـ TOTP (توليد الـ Secret و QR Code للمستخدم)
        /// </summary>
        [HttpPost("setup-mfa")]
        public async Task<IActionResult> SetupMfa([FromBody] SetupMfaCommand command, CancellationToken cancellationToken)
        {
            var result = await _sender.Send(command, cancellationToken);

            if (result.IsFailure)
            {
                return BadRequest(new { error = result.Error.ErrorMessage });
            }

            return Ok(result.Value);
        }

        /// <summary>
        /// خطوة تأكيد وتفعيل الـ TOTP عبر إدخال أول رمز صحيح من التطبيق
        /// </summary>
        [HttpPost("verify-and-enable-mfa")]
        public async Task<IActionResult> VerifyAndEnableMfa([FromBody] VerifyAndEnableMfaCommand command, CancellationToken cancellationToken)
        {
            var result = await _sender.Send(command, cancellationToken);

            if (result.IsFailure)
            {
                return BadRequest(new { error = result.Error.ErrorMessage });
            }

            return Ok(new { success = true, message = "MFA has been successfully verified and enabled." });
        }

        [HttpPost("change-mfa-preference")]
        public async Task<IActionResult> ChangeMfaPreference([FromBody] ChangeMfaPreferenceCommand command, CancellationToken cancellationToken)
        {
            var result = await _sender.Send(command, cancellationToken);
            if (result.IsFailure) return BadRequest(new { error = result.Error.ErrorMessage });
            return Ok(new { success = true, message = "MFA preference updated successfully." });
        }
    }
}
