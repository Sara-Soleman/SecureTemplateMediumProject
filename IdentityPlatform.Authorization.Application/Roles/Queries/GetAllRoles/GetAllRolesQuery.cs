using Common.Application.Abstractions.CQRS;
using Common.Domain.Errors;
using CSharpFunctionalExtensions;
using IdentityPlatform.Authorization.Application.Roles.Dtos;
using System;
using System.Collections.Generic;
using System.Text;

namespace IdentityPlatform.Authorization.Application.Roles.Queries.GetAllRoles
{
    public sealed record GetAllRolesQuery() : IQuery<IEnumerable<RoleDto>>;
}
