using System;
using System.Collections.Generic;
using System.Text;

namespace IdentityPlatform.Identity.Application.Users.Dtos
{
    public record UserProfileDto(
        Guid Id,
        string Username,
        string Email
    );
}
