using Common.Application.Abstractions.CQRS;
using IdentityPlatform.Identity.Domain.Dto;
using System;
using System.Collections.Generic;
using System.Text;

namespace IdentityPlatform.Identity.Application.Users.Commands.Login
{
    public sealed record VerifyLoginMfaCommand(
        Guid UserId,
        string Code
    ) : ICommand<AuthenticationResponseDto>; // استبدل DTO بالاسم لديك للتوكنات النهائية
}

