using Common.Application.Abstractions.CQRS;
using Common.Domain.Errors;
using CSharpFunctionalExtensions;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;

namespace IdentityPlatform.Identity.Application.MFA.VerifyAndEnableMfa
{
    public sealed record VerifyAndEnableMfaCommand(Guid UserId, string Code) : ICommand<bool>;
}
