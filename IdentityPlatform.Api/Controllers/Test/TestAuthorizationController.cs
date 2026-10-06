using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace IdentityPlatform.Api.Controllers.Test
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestAuthorizationController : ControllerBase
    {
        // 1. اختبار الوصول المفتوح لأي مستخدم مسجل دخول (Authenticated فقط)
        [HttpGet("ping")]
        [Authorize]
        public IActionResult Ping()
        {
            var userName = User.Identity?.Name ?? "Unknown";
            return Ok(new { message = $"Hello {userName}, you are authenticated successfully!" });
        }

        // 2. اختبار صلاحية إدارة المستخدمين (تتطلب Permissions.Users.Manage أو حسب طريقتك)
        [HttpGet("manage-users")]
        [Authorize(Roles = "SuperAdmin")] // أو يمكنك استخدام Policy للـ Permission
        public IActionResult ManageUsers()
        {
            return Ok(new { message = "Access granted! You have SuperAdmin role / permissions to manage users." });
        }

        // 3. اختبار صلاحية وهمية لا يملكها المستخدم (لتجربة الحظر 403)
        [HttpGet("restricted-area")]
        [Authorize(Roles = "NonExistentRoleForTesting")]
        public IActionResult RestrictedArea()
        {
            return Ok(new { message = "You should not be able to see this." });
        }
    }
}
