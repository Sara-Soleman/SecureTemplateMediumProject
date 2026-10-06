using Common.Application.Abstractions;
using Common.Application.Abstractions.CQRS;
using Common.Application.Abstractions.DomainEvents;
using Common.Application.Abstractions.Handlers;
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

namespace IdentityPlatform.Authorization.Application.Roles.Commands.RemoveRoleFromUser
{
    public class RemoveRoleFromUserCommandHandler : CommandHandlerBase<RemoveRoleFromUserCommand, Unit, IAuthorizationUnitOfWork>
    {
        private readonly IRoleRepository _roleRepository;
        private Role _role;

        public RemoveRoleFromUserCommandHandler(IRoleRepository roleRepository, IAuthorizationUnitOfWork unitOfWork,
            IDomainEventDispatcher domainEventDispatcher)
            : base(domainEventDispatcher, unitOfWork)
            {
            _roleRepository = roleRepository;
            }

        protected async override Task<Result<Unit, IDomainError>> ExecuteAsync(RemoveRoleFromUserCommand request, CancellationToken cancellationToken)
        {
            var userRole = await _roleRepository.GetUserRoleAsync(request.UserId, request.RoleId, cancellationToken);
            if (userRole == null)
            {
                return Result.Failure<Unit, IDomainError>(DomainError.UserRoleNotFound());
            }

            _roleRepository.RemoveUserRole(userRole);
           

            return Result.Success<Unit, IDomainError>(Unit.Value);
        }

        protected override IAggregateRoot? GetAggregateRoot(Result<Unit, IDomainError> result)
        {
            return _role;
        }
    }
}
