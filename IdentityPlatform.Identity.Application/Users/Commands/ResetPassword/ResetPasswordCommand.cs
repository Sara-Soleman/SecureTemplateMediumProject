using Common.Application.Abstractions.CQRS;
using System;
using System.Collections.Generic;
using System.Text;

namespace IdentityPlatform.Identity.Application.Users.Commands.ResetPassword
{
    public sealed record ResetPasswordCommand(string Email, string Token, string NewPassword) : ICommand<bool>;
}
