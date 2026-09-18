using Common.Application.Abstractions.CQRS;
using System;
using System.Collections.Generic;
using System.Text;

namespace IdentityPlatform.Identity.Application.Users.Commands.ForgotPassword
{
    public sealed record ForgotPasswordCommand(string Email) : ICommand<bool>;
}
