using Common.Application.Abstractions.CQRS;
using Common.Domain.Errors;
using CSharpFunctionalExtensions;
using IdentityPlatform.Identity.Domain.Dto;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace IdentityPlatform.Identity.Application.MFA.SetupMfa
{
    public sealed record SetupMfaCommand(Guid UserId) : ICommand<MfaSetupResponseDto>;
}
