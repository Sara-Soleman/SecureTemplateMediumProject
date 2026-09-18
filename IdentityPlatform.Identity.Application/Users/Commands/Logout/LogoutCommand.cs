using Common.Application.Abstractions.CQRS;
using IdentityPlatform.Identity.Application.Users.Commands.Login;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace IdentityPlatform.Identity.Application.Users.Commands.Logout
{
    
    public sealed record LogoutCommand(string RefreshToken) : ICommand<bool>;
}
