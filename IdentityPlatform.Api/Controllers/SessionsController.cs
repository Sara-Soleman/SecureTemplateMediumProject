using Common.Domain;
using IdentityPlatform.Api.Middleware;
using IdentityPlatform.Identity.Application.Sessions;
using IdentityPlatform.Identity.Application.Sessions.RevokeAllUserSessions;
using IdentityPlatform.Identity.Application.Sessions.RevokeSession;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using System.Security.Claims;

namespace IdentityPlatform.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SessionsController : ControllerBase
    {
        private readonly ISender _mediator; 
        private readonly IStringLocalizer _localizer;
        public SessionsController(ISender mediator, IStringLocalizerFactory factory)
        {
            _mediator = mediator;
            _localizer = factory.Create("SharedResources", "IdentityPlatform.Api");
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetMyActiveSessions(CancellationToken cancellationToken)
        {
            var userIdClaim = User.FindFirst("sub")?.Value
                              ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!Guid.TryParse(userIdClaim, out var parsedGuid))
            {
                string faildMessage = _localizer["Unauthorized"];

                return Ok(new { message = faildMessage });
                //return Unauthorized(new { message = "Invalid user identity claim." });
            }

            var userId = new Id<Guid>(parsedGuid);
            var query = new GetActiveSessionsQuery(userId);

            // تنفيذ الـ Query عبر الـ Mediator (مع الانتباه لبصمة الارجاع المزدوجة لديك)
            var result = await _mediator.Send(query, cancellationToken);

            // التعامل مع التغليف المزدوج للـ Result (بما يتوافق مع هيكلة مشروعك)
           
           
            if (result.IsFailure)
            {
                return result.ToLocalizedErrorResult(_localizer);
                //return BadRequest(result.Error);
            }

            // إذا تم النجاح، أرجع البيانات الحقيقية فقط (innerResult.Value) بدلاً من كائن الـ Result كله
            return Ok(result.Value);
        }

        [HttpDelete("{sessionId:guid}")]
        public async Task<IActionResult> RevokeSession(Guid sessionId, CancellationToken cancellationToken)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                              ?? User.FindFirst("sub")?.Value;

            if (!Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized();
            }

            var command = new RevokeSessionCommand(sessionId, userId);
            var result = await _mediator.Send(command, cancellationToken);

            if (result.IsFailure)
            {
                return result.ToLocalizedErrorResult(_localizer);
            }

            return NoContent();
        }

        [HttpDelete("all")]
        public async Task<IActionResult> RevokeAllSessions(CancellationToken cancellationToken)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                              ?? User.FindFirst("sub")?.Value;

            if (!Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized();
            }

            var command = new RevokeAllUserSessionsCommand(userId);
            var result = await _mediator.Send(command, cancellationToken);

            if (result.IsFailure)
            {
                return result.ToLocalizedErrorResult(_localizer);
            }

            return NoContent();
        }
    }
}
