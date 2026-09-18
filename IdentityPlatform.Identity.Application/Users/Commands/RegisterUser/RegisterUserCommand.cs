using Common.Application.Abstractions.CQRS;
using System;
using System.Collections.Generic;
using System.Text;

namespace IdentityPlatform.Identity.Application.Users.Commands.RegisterUser
{
    public sealed record RegisterUserCommand(
        string Username,
        string Email,
        string Password
    ) : ICommand<Guid>; // يُعيد معرف المستخدم Guid عند النجاح
}
