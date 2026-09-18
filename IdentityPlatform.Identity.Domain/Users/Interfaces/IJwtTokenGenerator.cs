using IdentityPlatform.Identity.Domain.Dto;
using System;
using System.Collections.Generic;
using System.Text;

namespace IdentityPlatform.Identity.Domain.Users.Interfaces
{
    public interface IJwtTokenGenerator
    {
        string GenerateToken(User user, Guid sessionId);
        Task<AuthenticationResponseDto> GenerateTokensAsync(User user,
            string ipAddress = "Unknown",
            string userAgent = "Unknown",
           CancellationToken cancellationToken = default);
    }
}
