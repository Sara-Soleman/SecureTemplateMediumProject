using Common.Application.Abstractions.CQRS;
using Common.Domain.Errors;
using IdentityPlatform.Identity.Domain.Tokens.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace IdentityPlatform.Identity.Application.Users.Commands.RefreshToken
{
    /// <summary>
    /// Represents a command to refresh an access token using a refresh token.
    /// </summary>
    public sealed record RefreshTokenCommand(string RefreshToken, string IpAddress) : ICommand<AuthResponseDto>;
}
