using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace IdentityPlatform.Identity.Application.Users.Commands.Login
{
    public class LoginCommandValidator : AbstractValidator<LoginCommand>
    {
        public LoginCommandValidator()
        {
           
            
            RuleFor(x => x.UsernameOrEmail)
              
                .NotEmpty().WithMessage("EmailRequired")
                .EmailAddress().WithMessage("EmailInvalid");

            RuleFor(x => x.Password)
            .NotEmpty().WithMessage("PasswordRequired");
        }
    }
}
