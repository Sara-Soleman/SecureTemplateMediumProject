using IdentityPlatform.Authorization.Application.Roles.Commands.AssignRoleToUser;
using IdentityPlatform.Authorization.Application.Roles.Commands.CreateRole;
using IdentityPlatform.Authorization.Application.Roles.Commands.RemoveRoleFromUser;
using IdentityPlatform.Authorization.Application.Roles.Queries.GetAllRoles;
using IdentityPlatform.Authorization.Application.Roles.Queries.GetUserRoles;
using IdentityPlatform.Authorization.Domain;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace IdentityPlatform.Api.Controllers.Roles
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Policy = Permissions.Roles.Manage)]

    public class RolesController : ControllerBase
    {
        private readonly ISender _sender;
        private readonly IStringLocalizer _localizer;

        public RolesController(ISender sender, IStringLocalizerFactory factory)
        {
            _sender = sender;
            _localizer = factory.Create("SharedResources", "IdentityPlatform.Api");
        }
        

        [HttpPost("create")]
        public async Task<IActionResult> CreateRole([FromBody, FromServices] CreateRoleCommand command, CancellationToken cancellationToken)
        {
            var result = await _sender.Send(command, cancellationToken);

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

            return Ok(new { RoleId = result.Value });
        }
        /// <summary>
        /// إسناد دور معين لمستخدم
        /// </summary>
        [HttpPost("assign-to-user")]
        public async Task<IActionResult> AssignRoleToUser([FromBody] AssignRoleToUserCommand command, CancellationToken cancellationToken)
        {
            var result = await _sender.Send(command, cancellationToken);

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
            var Message = _localizer["Roleassignedtouser"].Value;


            return Ok(Message);
            
        }

        /// <summary>
        /// جلب جميع الأدوار والصلاحيات الخاصة بمستخدم معين
        /// </summary>
        [HttpGet("user/{userId:guid}")]
        public async Task<IActionResult> GetUserRoles(Guid userId, CancellationToken cancellationToken)
        {
            var result = await _sender.Send(new GetUserRolesQuery(userId), cancellationToken);

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

            return Ok(result.Value);
        }
        /// <summary>
        /// جلب جميع الأدوار والصلاحيات الخاصة بمستخدم معين
        /// </summary>
        [HttpGet("getAllRoles")]
        public async Task<IActionResult> GetAllRoles(CancellationToken cancellationToken)
        {
            var result = await _sender.Send(new GetAllRolesQuery(), cancellationToken);

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

            return Ok(result.Value);
        }

        /// <summary>
        /// إزالة دور من مستخدم
        /// </summary>
        [HttpDelete("remove-from-user")]
        public async Task<IActionResult> RemoveRoleFromUser([FromBody] RemoveRoleFromUserCommand command, CancellationToken cancellationToken)
        {
            var result = await _sender.Send(command, cancellationToken);

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


            var Message = _localizer["Roleremoved"].Value;
           

            return Ok(Message);
          
        }
    }
}

