using Common.Application.Abstractions;
using Common.Application.Abstractions.CQRS;
using Common.Application.Abstractions.DomainEvents;
using Common.Application.Abstractions.Handlers;
using Common.Application.Events.Dispatchers;
using Common.Domain;
using Common.Domain.Errors;
using CSharpFunctionalExtensions;
using IdentityPlatform.Authorization.Application.Persistence;
using IdentityPlatform.Authorization.Domain.Roles;
using IdentityPlatform.Authorization.Domain.Roles.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace IdentityPlatform.Authorization.Application.Roles.Commands.AssignRoleToUser
{
    public class AssignRoleToUserCommandHandler : CommandHandlerBase<AssignRoleToUserCommand, Unit, IAuthorizationUnitOfWork>
    {
        private readonly IRoleRepository _roleRepository;
        private Role _role;
        public AssignRoleToUserCommandHandler(IRoleRepository roleRepository, IAuthorizationUnitOfWork unitOfWork,
            IDomainEventDispatcher domainEventDispatcher)
            : base(domainEventDispatcher, unitOfWork) { 
            _roleRepository = roleRepository;
        }

       

        protected async override Task<Result<Unit, IDomainError>> ExecuteAsync(AssignRoleToUserCommand request, CancellationToken cancellationToken)
        {
            // 1. التحقق من أن الدور موجود
            var roleExists = await _roleRepository.ExistsAsync(request.RoleId, cancellationToken);
            if (!roleExists)
            {
                return Result.Failure<Unit, IDomainError>(DomainError.RoleNotFound());
            }

            // 2. التحقق مما إذا كان المستخدم يملك هذا الدور مسبقاً لمنع التكرار
            var hasRole = await _roleRepository.UserHasRoleAsync(request.UserId, request.RoleId, cancellationToken);
            if (hasRole)
            {
                return Result.Failure<Unit, IDomainError>(DomainError.UserAlreadyHasRole());
            }

            // 3. إنشاء ربط المستخدم بالدور باستخدام الـ Factory
            var userRole = UserRole.Create((request.UserId), new Id<Role>(request.RoleId));

            await _roleRepository.AddUserRoleAsync(userRole, cancellationToken);
            return Result.Success<Unit, IDomainError>(Unit.Value);
        }

        protected override IAggregateRoot? GetAggregateRoot(Result<Unit, IDomainError> result)
        {
            return _role;
        }
    }
}
