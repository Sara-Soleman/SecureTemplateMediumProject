using Common.Application.Abstractions.CQRS;
using Common.Domain.Errors;
using CSharpFunctionalExtensions;
using IdentityPlatform.Authorization.Application.Roles.Dtos;
using IdentityPlatform.Authorization.Domain.Roles.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace IdentityPlatform.Authorization.Application.Roles.Queries.GetUserRoles
{
    public class GetUserRolesQueryHandler : IQueryHandler<GetUserRolesQuery, IEnumerable<RoleDto>>
    {
        private readonly IRoleRepository _roleRepository;

        public GetUserRolesQueryHandler(IRoleRepository roleRepository)
        {
            _roleRepository = roleRepository;
        }

        public async Task<Result<IEnumerable<RoleDto>, IDomainError>> Handle(GetUserRolesQuery request, CancellationToken cancellationToken)
        {
            var roles = await _roleRepository.GetRolesByUserIdAsync(request.UserId, cancellationToken);

            var roleDtos = roles.Select(r => new RoleDto(
                r.Id.Value,
                r.Name,
                r.Description,
                r.Permissions
            ));

            return Result.Success<IEnumerable<RoleDto>, IDomainError>(roleDtos);
        }
    }
}
