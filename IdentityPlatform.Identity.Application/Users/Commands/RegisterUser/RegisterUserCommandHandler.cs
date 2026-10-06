using Common.Application.Abstractions;
using Common.Application.Abstractions.CQRS;
using Common.Application.Abstractions.DomainEvents;
using Common.Application.Abstractions.Handlers;
using Common.Domain;
using Common.Domain.Errors;
using CSharpFunctionalExtensions;
using IdentityPlatform.Identity.Application.Persistence;
using IdentityPlatform.Identity.Domain.Users;
using IdentityPlatform.Identity.Domain.Users.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace IdentityPlatform.Identity.Application.Users.Commands.RegisterUser
{
    public sealed class RegisterUserCommandHandler : CommandHandlerBase<RegisterUserCommand, Guid, IIdentityUnitOfWork>
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher _passwordHasher;
        private User _user;

        public RegisterUserCommandHandler(
                IUserRepository userRepository,
                IPasswordHasher passwordHasher,
                IIdentityUnitOfWork unitOfWork,
                IDomainEventDispatcher domainEventDispatcher)
                    : base(domainEventDispatcher, unitOfWork)
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
        }

       
        protected async override Task<Result<Guid, IDomainError>> ExecuteAsync(RegisterUserCommand request, CancellationToken cancellationToken)
        {
            // 1. التحقق إن كان المستخدم موجوداً مسبقاً
            var exists = await _userRepository.ExistsByUsernameOrEmailAsync(
                request.Username,
                request.Email,
                cancellationToken);

            if (exists)
            {
                // إرجاع خطأ نطاق (Domain Error) بدلاً من رمي Exception
                // (استبدل UserErrors.EmailOrUsernameAlreadyExists بالخطأ المعرف لديك في الدومين)
                return Result.Failure<Guid, IDomainError>(DomainError.EmailOrUsernameAlreadyExists());
            }

            // 2. تشفير كلمة المرور باستخدام واجهة IPasswordHasher
            var passwordHash = _passwordHasher.HashPassword(request.Password);

            // 3. إنشاء كائن User
            var user = User.Create(request.Username, request.Email, passwordHash);

            // 4. إضافة المستخدم إلى الـ Repository
            await _userRepository.AddAsync(user, cancellationToken);

            // إرجاع النتيجة بنجاح مغلفة بـ Result
            return Result.Success<Guid, IDomainError>(user.Id.Value);
        }

        protected override IAggregateRoot? GetAggregateRoot(Result<Guid, IDomainError> result)
        {
            return _user;
        }
    }
}
