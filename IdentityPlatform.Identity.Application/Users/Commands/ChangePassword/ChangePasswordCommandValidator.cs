using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace IdentityPlatform.Identity.Application.Users.Commands.ChangePassword
{
    public sealed class ChangePasswordCommandValidator : AbstractValidator<ChangePasswordCommand>
    {
        public ChangePasswordCommandValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty();

            RuleFor(x => x.CurrentPassword)
                .NotEmpty().WithMessage("PasswordRequired");

            RuleFor(x => x.NewPassword)
                .NotEmpty().WithMessage("NewPasswordRequired")
                .MinimumLength(8).WithMessage("PasswordShort")
                .NotEqual(x => x.CurrentPassword).WithMessage("IdenticalPassword")
                .Matches("[A-Z]").WithMessage("PasswordMissingUppercase")
                .Matches("[0-9]").WithMessage("PasswordMissingNumber");
        }
    }
}
