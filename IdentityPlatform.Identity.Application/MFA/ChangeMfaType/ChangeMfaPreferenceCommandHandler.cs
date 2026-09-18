using Common.Application.Abstractions;
using Common.Application.Abstractions.CQRS;
using Common.Domain.Errors;
using CSharpFunctionalExtensions;
using IdentityPlatform.Identity.Domain.Users.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace IdentityPlatform.Identity.Application.MFA.ChangeMfaType
{
    public sealed class ChangeMfaPreferenceCommandHandler : ICommandHandler<ChangeMfaPreferenceCommand, bool>
    {
        private readonly IUserRepository _userRepository;
        private readonly IUnitOfWork _unitOfWork;

        public ChangeMfaPreferenceCommandHandler(
            IUserRepository userRepository,
            IUnitOfWork unitOfWork)
        {
            _userRepository = userRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<bool, IDomainError>> Handle(ChangeMfaPreferenceCommand request, CancellationToken cancellationToken)
        {
            // 1. جلب المستخدم من قاعدة البيانات
            var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
            if (user == null)
            {
                return Result.Failure<bool, IDomainError>(DomainError.UserNotFound());
            }

            // 2. تحديث قناة الـ MFA المفضلة باستخدام دالة في Domain Model (لتطبيق حماية الـ Encapsulation)
            // (تأكد من توفر دالة داخل كلاس User تقوم بتحديثها، أو تعديل الخاصية مباشرة إذا كانت متاحًة)
            var updateResult = user.ChangePreferredMfaType(request.NewMfaType);
            if (updateResult.IsFailure)
            {
                return Result.Failure<bool, IDomainError>(updateResult.Error);
            }

            // 3. حفظ التغييرات في قاعدة البيانات
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success<bool, IDomainError>(true);
        }
    }
}
