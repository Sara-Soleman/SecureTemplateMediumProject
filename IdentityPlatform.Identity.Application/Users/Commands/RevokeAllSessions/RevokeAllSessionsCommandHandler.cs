using Common.Application.Abstractions;
using Common.Domain.Errors;
using CSharpFunctionalExtensions;
using IdentityPlatform.Identity.Domain.Users.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace IdentityPlatform.Identity.Application.Users.Commands.RevokeAllSessions
{
    public sealed class RevokeAllSessionsCommandHandler : IRequestHandler<RevokeAllSessionsCommand, Result<bool, IDomainError>>
    {
        private readonly IUserRepository _userRepository;
        private readonly IUnitOfWork _unitOfWork;

        public RevokeAllSessionsCommandHandler(
            IUserRepository userRepository,
            IUnitOfWork unitOfWork)
        {
            _userRepository = userRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<bool, IDomainError>> Handle(RevokeAllSessionsCommand request, CancellationToken cancellationToken)
        {
            // 1. جلب المستخدم
            var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
            if (user == null)
            {
                return Result.Failure<bool, IDomainError>(DomainError.UserNotFound());
            }

            // 2. استدعاء ميثود زيادة الإصدار الموجودة في الكيان لإبطال كافة التوكنات السابقة
            user.IncrementTokenVersion();

            // 3. حفظ التغييرات
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success<bool, IDomainError>(true);
        }
    }
}
