using Common.Application.Abstractions;
using Common.Application.Abstractions.CQRS;
using Common.Application.Abstractions.DomainEvents;
using Common.Application.Abstractions.Handlers;
using Common.Domain;
using Common.Domain.Errors;
using CSharpFunctionalExtensions;
using IdentityPlatform.Identity.Domain.Users;
using IdentityPlatform.Identity.Domain.Users.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace IdentityPlatform.Identity.Application.MFA.ChangeMfaType
{
    public sealed class ChangeMfaPreferenceCommandHandler : CommandHandlerBase<ChangeMfaPreferenceCommand, bool>
    {
        private readonly IUserRepository _userRepository;
        private User _user;


        public ChangeMfaPreferenceCommandHandler(
            IUserRepository userRepository,
             IUnitOfWork unitOfWork,
            IDomainEventDispatcher domainEventDispatcher)
            : base(domainEventDispatcher, unitOfWork)
        {
            _userRepository = userRepository;
        }

       

        protected async override Task<Result<bool, IDomainError>> ExecuteAsync(ChangeMfaPreferenceCommand request, CancellationToken cancellationToken)
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


            return Result.Success<bool, IDomainError>(true);
        }

        

        protected override IAggregateRoot? GetAggregateRoot(Result<bool, IDomainError> result)
        {
            // Return the created aggregate root to dispatch domain events
            return _user;
        }

    }
}

