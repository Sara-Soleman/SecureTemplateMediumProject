using Common.Domain.Errors;
using CSharpFunctionalExtensions;
using IdentityPlatform.Identity.Application.Users.Dtos;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace IdentityPlatform.Identity.Application.Users.Queries.GetUserProfile
{
    public record GetMyProfileQuery() : IRequest<Result<UserProfileDto, IDomainError>>;
}
