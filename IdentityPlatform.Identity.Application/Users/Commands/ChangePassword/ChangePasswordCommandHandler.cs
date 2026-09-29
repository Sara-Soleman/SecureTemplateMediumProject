using Common.Application.Abstractions;
using Common.Application.Abstractions.DomainEvents;
using Common.Application.Abstractions.Handlers;
using Common.Domain;
using Common.Domain.Errors;
using CSharpFunctionalExtensions;
using IdentityPlatform.Identity.Domain.Users;
using IdentityPlatform.Identity.Domain.Users.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace IdentityPlatform.Identity.Application.Users.Commands.ChangePassword
{
    public sealed class ChangePasswordCommandHandler : CommandHandlerBase<ChangePasswordCommand, bool>
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher _passwordHasher;
        private User _user;

        public ChangePasswordCommandHandler(
            IUserRepository userRepository,
            IPasswordHasher passwordHasher,
            IUnitOfWork unitOfWork,
            IDomainEventDispatcher domainEventDispatcher)
            : base(domainEventDispatcher, unitOfWork)
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
           
        }


        protected async override Task<Result<bool, IDomainError>> ExecuteAsync(ChangePasswordCommand request, CancellationToken cancellationToken)
        {
            // 1. جلب المستخدم مع بياناته (تأكد من استخدام الميثود المناسبة لجلب الـ User مع الـ Credential)
            var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken); // أو GetByIdWithDetailsAsync بحسب ما هو متاح لديك
            if (user == null || user.Credential == null)
            {
                return Result.Failure<bool, IDomainError>(DomainError.UserNotFound());
            }

            // 2. التحقق من صحة كلمة المرور الحالية
            var isCurrentPasswordValid = _passwordHasher.VerifyPassword(request.CurrentPassword, user.Credential.PasswordHash);
            if (!isCurrentPasswordValid)
            {
                return Result.Failure<bool, IDomainError>(DomainError.InvalidCurrentPassword());
            }

            // 3. تشفير كلمة المرور الجديدة
            var newPasswordHash = _passwordHasher.HashPassword(request.NewPassword);

            // 4. استخدام دوال الكيان الجاهزة (تحديث كلمة المرور وزيادة الإصدار لإبطال الجلسات القديمة)
            user.ChangePassword(newPasswordHash);
            user.IncrementTokenVersion(); //  
            user.RevokeAllRefreshFamilies();

            return Result.Success<bool, IDomainError>(true);
        }

        protected override IAggregateRoot? GetAggregateRoot(Result<bool, IDomainError> result)
        {
            return _user;
        }
    }
}
