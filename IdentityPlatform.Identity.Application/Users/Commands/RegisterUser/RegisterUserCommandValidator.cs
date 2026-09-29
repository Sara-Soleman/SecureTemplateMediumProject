using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace IdentityPlatform.Identity.Application.Users.Commands.RegisterUser
{
    public sealed class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
    {
        public RegisterUserCommandValidator()
        {
            RuleFor(x => x.Username)
                .NotEmpty().WithMessage("UsernameRequired")
                .MaximumLength(100).WithMessage("UsernameLong");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("EmailRequired")
                .EmailAddress().WithMessage("EmailInvalid");

            RuleFor(x => x.Password)
            .NotEmpty().WithMessage("PasswordRequired")
            .MinimumLength(8).WithMessage("PasswordShort")
            .Matches("[A-Z]").WithMessage("PasswordMissingUppercase")
            .Matches("[0-9]").WithMessage("PasswordMissingNumber");
        }
    }
}
