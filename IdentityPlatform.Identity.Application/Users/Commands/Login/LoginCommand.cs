using Common.Application.Abstractions.CQRS;
using Common.Domain.Errors;
using CSharpFunctionalExtensions;
using IdentityPlatform.Identity.Domain.Dto;
using System;
using System.Collections.Generic;
using System.Text;

namespace IdentityPlatform.Identity.Application.Users.Commands.Login
{
    public sealed record LoginCommand(
        string UsernameOrEmail,
        string Password
    ) : ICommand<MfaChallengeResponse>;

    public sealed record LoginCommand1(
        string UsernameOrEmail,
        string Password,
        string IpAddress,
        string UserAgent
    ) : ICommand<MfaChallengeResponse>;
    public sealed record LoginResult(
        string AccessToken
    );
}
