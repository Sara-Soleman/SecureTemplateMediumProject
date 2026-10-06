using IdentityPlatform.Api.Middleware;
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
using IdentityPlatform.Identity.Application.Users.Dtos;
using IdentityPlatform.Identity.Application.Users.Queries.GetUserProfile;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Localization;
using System.Security.Claims;

namespace IdentityPlatform.Api.Controllers
{
    [Route("api/[controller]")]
    [EnableRateLimiting("FixedGlobal")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ISender _sender;
        private readonly IStringLocalizer _localizer;

        public AuthController(ISender sender, IStringLocalizerFactory factory)
        {
            _sender = sender;
            _localizer = factory.Create("SharedResources", "IdentityPlatform.Api");
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

                var response = result;


                var localizedMainMessage = _localizer[response.Error.ErrorMessage];

               
                var localizedErrors = response.Error.Errors?
                    .Select(errKey => _localizer[errKey].Value) 
                    .ToList() ?? new List<string>();

                return BadRequest(new
                {
                    code = response.Error.GetHashCode(),
                    message = localizedMainMessage.Value,
                    errors = localizedErrors            
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

                var ErrorResponse = result;


               
                var localizedMainMessage = _localizer[ErrorResponse.Error.ErrorMessage];

                var localizedErrors = ErrorResponse.Error.Errors?
                    .Select(errKey => _localizer[errKey].Value)
                    .ToList() ?? new List<string>();

                return BadRequest(new
                {
                    code = ErrorResponse.Error.GetHashCode(),
                    message = localizedMainMessage.Value,
                    errors = localizedErrors
                });
            }
            //if (result.IsFailure)
            //{

            //    return result.ToLocalizedErrorResult(_localizer);
            //   // return Unauthorized(new { error = result.Error.ErrorMessage });
            //}

            var response = result.Value;

            
            var localizedResponse = new
            {
                response.UserId,
                response.MfaType,
                Message = _localizer[response.Message].Value
            };

            return Ok(localizedResponse);

           // return Ok(result.Value);
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

                var ErrorResponse = result;


                if (ErrorResponse.Error.Errors.Count == 1)
                {
                    return BadRequest(new
                    {
                        code = ErrorResponse.Error.GetHashCode(),
                        message = result.ToLocalizedErrorResult(_localizer)
                    });
                }
                var localizedMainMessage = _localizer[ErrorResponse.Error.ErrorMessage];

                var localizedErrors = ErrorResponse.Error.Errors?
                    .Select(errKey => _localizer[errKey].Value)
                    .ToList() ?? new List<string>();

                return BadRequest(new
                {
                    code = ErrorResponse.Error.GetHashCode(),
                    message = localizedMainMessage.Value,
                    errors = localizedErrors
                });
            }


            return Ok(result.Value);
        }

        /// <label>تجديد التوكن (Token Rotation)</label>
        [HttpPost("refresh-token")]
        [Authorize]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequestDto request, CancellationToken cancellationToken)
        {
            var rawIp = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "0.0.0.1";
            var normalizedIp = NormalizeIpAddress(rawIp);

            var command = new RefreshTokenCommand(
                RefreshToken: request.RefreshToken,
                IpAddress: normalizedIp,
                UserAgent: Request.Headers["User-Agent"].ToString()
            );

            var result = await _sender.Send(command, cancellationToken);

            if (result.IsFailure)
            {

                var ErrorResponse = result;


                var localizedMainMessage = _localizer[ErrorResponse.Error.ErrorMessage];

                var localizedErrors = ErrorResponse.Error.Errors?
                    .Select(errKey => _localizer[errKey].Value)
                    .ToList() ?? new List<string>();
                //return BadRequest(result.Error);
                return BadRequest(new
                {
                    code = ErrorResponse.Error.GetHashCode(),
                    message = localizedMainMessage.Value,
                    errors = localizedErrors
                });
            }



            return Ok(result.Value);
        }
        public static string NormalizeIpAddress(string? ipAddress)
        {
            if (string.IsNullOrWhiteSpace(ipAddress))
                return "0.0.0.1";

            // تحويل IPv6 Loopback إلى IPv4 القياسي لكي يتطابق محلياً
            if (ipAddress == "::1")
                return "0.0.0.1";

            // إزالة بادئة IPv6 الملتصقة أحياناً ::ffff:
            if (ipAddress.StartsWith("::ffff:"))
                return ipAddress.Substring(7);

            return ipAddress;
        }


        /// <label>طلب استعادة كلمة المرور (إرسال الرمز)</label>
        [HttpPost("forgot-password")]
        [Authorize]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordCommand command, CancellationToken cancellationToken)
        {
            var result = await _sender.Send(command, cancellationToken);

            if (result.IsFailure)
            {

                var ErrorResponse = result;

                var localizedMainMessage = _localizer[ErrorResponse.Error.ErrorMessage];

                var localizedErrors = ErrorResponse.Error.Errors?
                    .Select(errKey => _localizer[errKey].Value)
                    .ToList() ?? new List<string>();

                return BadRequest(new
                {
                    code = ErrorResponse.Error.GetHashCode(),
                    message = localizedMainMessage.Value,
                    errors = localizedErrors
                });
            }

            return Ok(result.Value);
        }

        /// <label>إعادة تعيين كلمة المرور باستخدام الرمز</label>
        [HttpPost("reset-password")]
        [EnableRateLimiting("StrictAuth")]
        [Authorize]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordCommand command, CancellationToken cancellationToken)
        {
            var result = await _sender.Send(command, cancellationToken);

            if (result.IsFailure)
            {

                var ErrorResponse = result;

                var localizedMainMessage = _localizer[ErrorResponse.Error.ErrorMessage];

                var localizedErrors = ErrorResponse.Error.Errors?
                    .Select(errKey => _localizer[errKey].Value)
                    .ToList() ?? new List<string>();

                return BadRequest(new
                {
                    code = ErrorResponse.Error.GetHashCode(),
                    message = localizedMainMessage.Value,
                    errors = localizedErrors
                });
            }

            return Ok(result.Value);
        }

        /// <label>تسجيل الخروج (إبطال عائلة التوكنات)</label>
        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout([FromBody] LogoutCommand command, CancellationToken cancellationToken)
        {
            var result = await _sender.Send(command, cancellationToken);

            if (result.IsFailure)
            {

                var ErrorResponse = result;


                var localizedMainMessage = _localizer[ErrorResponse.Error.ErrorMessage];

                var localizedErrors = ErrorResponse.Error.Errors?
                    .Select(errKey => _localizer[errKey].Value)
                    .ToList() ?? new List<string>();

                return BadRequest(new
                {
                    code = ErrorResponse.Error.GetHashCode(),
                    message = localizedMainMessage.Value,
                    errors = localizedErrors
                });
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

                var ErrorResponse = result;


                var localizedMainMessage = _localizer[ErrorResponse.Error.ErrorMessage];

                var localizedErrors = ErrorResponse.Error.Errors?
                    .Select(errKey => _localizer[errKey].Value)
                    .ToList() ?? new List<string>();

                return BadRequest(new
                {
                    code = ErrorResponse.Error.GetHashCode(),
                    message = localizedMainMessage.Value,
                    errors = localizedErrors
                });
            }
            string successMessage = _localizer["PasswordChange"];
            return Ok(new { message = successMessage });
        }

        public sealed record ChangePasswordRequestDto(
    string CurrentPassword,
    string NewPassword
);

        ///// <summary>
        ///// إلغاء جميع جلسات المستخدم من كافة الأجهزة (Revoke All Sessions)
        ///// </summary>
        //[Authorize] // تتطلب تسجيل الدخول
        //[HttpPost("revoke-all-sessions")]
        //public async Task<IActionResult> RevokeAllSessions(CancellationToken cancellationToken)
        //{
        //    // استخراج معرف المستخدم الحالي من الـ Claims
        //    if (!TryGetUserId(out var userId))
        //    {
        //        return Unauthorized();
        //    }

        //    var command = new RevokeAllSessionsCommand(userId);

        //    var result = await _sender.Send(command, cancellationToken);

        //    if (result.IsFailure)
        //    {

        //        var ErrorResponse = result;

        //        var localizedMainMessage = _localizer[ErrorResponse.Error.ErrorMessage];

        //        var localizedErrors = ErrorResponse.Error.Errors?
        //            .Select(errKey => _localizer[errKey].Value)
        //            .ToList() ?? new List<string>();

        //        return BadRequest(new
        //        {
        //            code = ErrorResponse.Error.GetHashCode(),
        //            message = localizedMainMessage.Value,
        //            errors = localizedErrors
        //        });
        //    }

        //    string successMessage = _localizer["SessionLogoutSuccessfully"];

        //    return Ok(new { message = successMessage });
           
        //}

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

                var ErrorResponse = result;

                var localizedMainMessage = _localizer[ErrorResponse.Error.ErrorMessage];

                var localizedErrors = ErrorResponse.Error.Errors?
                    .Select(errKey => _localizer[errKey].Value)
                    .ToList() ?? new List<string>();

                return BadRequest(new
                {
                    code = ErrorResponse.Error.GetHashCode(),
                    message = localizedMainMessage.Value,
                    errors = localizedErrors
                });
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

                var ErrorResponse = result;


                //if (ErrorResponse.Error.Errors.Count == 1)
                //{
                //    return BadRequest(new
                //    {
                //        code = ErrorResponse.Error.GetHashCode(),
                //        message = result.ToLocalizedErrorResult(_localizer)
                //    });
                //}
                var localizedMainMessage = _localizer[ErrorResponse.Error.ErrorMessage];

                var localizedErrors = ErrorResponse.Error.Errors?
                    .Select(errKey => _localizer[errKey].Value)
                    .ToList() ?? new List<string>();

                return BadRequest(new
                {
                    code = ErrorResponse.Error.GetHashCode(),
                    message = localizedMainMessage.Value,
                    errors = localizedErrors
                });
            }
            string successMessage = _localizer["MFASuccess"];

            return Ok(new { message = successMessage });
           // return Ok(new { success = true, message = "MFA has been successfully verified and enabled." });
        }

        [HttpPost("change-mfa-preference")]
        public async Task<IActionResult> ChangeMfaPreference([FromBody] ChangeMfaPreferenceCommand command, CancellationToken cancellationToken)
        {
            var result = await _sender.Send(command, cancellationToken);

            if (result.IsFailure)
            {

                var ErrorResponse = result;


                var localizedMainMessage = _localizer[ErrorResponse.Error.ErrorMessage];

                var localizedErrors = ErrorResponse.Error.Errors?
                    .Select(errKey => _localizer[errKey].Value)
                    .ToList() ?? new List<string>();

                return BadRequest(new
                {
                    code = ErrorResponse.Error.GetHashCode(),
                    message = localizedMainMessage.Value,
                    errors = localizedErrors
                });
            }

            string successMessage = _localizer["MFAUpdate"];

            return Ok(new { message = successMessage });
           // return Ok(new { success = true, message = "MFA preference updated successfully." });
        }


        
    }
}
