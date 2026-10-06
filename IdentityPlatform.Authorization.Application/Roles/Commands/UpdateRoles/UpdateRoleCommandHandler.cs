using Common.Application.Abstractions;
using Common.Application.Abstractions.DomainEvents;
using Common.Application.Abstractions.Handlers;
using Common.Domain;
using Common.Domain.Errors;
using CSharpFunctionalExtensions;
using IdentityPlatform.Authorization.Application.Persistence;
using IdentityPlatform.Authorization.Application.Resource_Based_Authorization;
using IdentityPlatform.Authorization.Application.Roles.Dtos;
using IdentityPlatform.Authorization.Domain.Roles;
using IdentityPlatform.Authorization.Domain.Roles.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace IdentityPlatform.Authorization.Application.Roles.Commands.UpdateRoles
{
    public class UpdateRoleCommandHandler : CommandHandlerBase<UpdateRoleCommand, Result, IAuthorizationUnitOfWork>
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IRoleRepository _roleRepository;
        private Role _role;
        public UpdateRoleCommandHandler(
            IAuthorizationService authorizationService,
            IHttpContextAccessor httpContextAccessor,
            IRoleRepository roleRepository,
             IAuthorizationUnitOfWork unitOfWork,
             IDomainEventDispatcher domainEventDispatcher)
            : base(domainEventDispatcher, unitOfWork){
            _authorizationService = authorizationService;
            _httpContextAccessor = httpContextAccessor;
            _roleRepository = roleRepository;
        }

       

        protected async override Task<Result<Result, IDomainError>> ExecuteAsync(UpdateRoleCommand request, CancellationToken cancellationToken)
        {
            // 1. جلب المورد (الدور) من قاعدة البيانات
            var role = await _roleRepository.GetByIdAsync(request.RoleId, cancellationToken);
            if (role == null) return Result.Failure(DomainError.NotFound().ErrorMessage);

            // تحويله لـ DTO أو استخدام الـ Entity مباشرة كمورد
            var roleDto = new RoleResponseDto { Id = role.Id, IsSystemRole = role.IsSystem, CreatedByUserId = role.CreatedBy };

            // 2. الحصول على الـ ClaimsPrincipal للمستخدم الحالي من الـ HttpContext
            var currentUserPrincipal = _httpContextAccessor.HttpContext?.User;
            if (currentUserPrincipal == null)
            {
                return Result.Failure(DomainError.Unauthorized().ErrorMessage);
            }
            // 2. فحص صلاحية الوصول للمورد (Resource-Based Authorization)
            var authResult = await _authorizationService.AuthorizeAsync(currentUserPrincipal, roleDto, new SameUserOrAdminRequirement());

            if (!authResult.Succeeded)
            {
                return Result.Failure(DomainError.Unauthorized().ErrorMessage);
            }
            role.UpdateDetails(request.Name, request.Description, request.Permissions);
            await _roleRepository.Update(role);
            // 3. متابعة منطق التعديل الحقيقي...
            return Result.Success();
        }

        protected override IAggregateRoot? GetAggregateRoot(Result<Result, IDomainError> result)
        {
            return _role;
        }
    }
}
