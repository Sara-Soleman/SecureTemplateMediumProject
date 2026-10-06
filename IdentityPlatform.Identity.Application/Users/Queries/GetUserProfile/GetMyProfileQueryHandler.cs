using Common.Domain;
using Common.Domain.Errors;
using CSharpFunctionalExtensions;
using IdentityPlatform.Identity.Application.Users.Dtos;
using IdentityPlatform.Identity.Domain.Users;
using IdentityPlatform.Identity.Domain.Users.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace IdentityPlatform.Identity.Application.Users.Queries.GetUserProfile
{
    public class GetMyProfileQueryHandler : IRequestHandler<GetMyProfileQuery, Result<UserProfileDto, IDomainError>>
    {
        private readonly ICurrentUserService _currentUserService;
        private readonly IUserRepository _userRepository; // أو الـ Repository الخاص بك

        public GetMyProfileQueryHandler(ICurrentUserService currentUserService, IUserRepository userRepository)
        {
            _currentUserService = currentUserService;
            _userRepository = userRepository;
        }

        public async Task<Result<UserProfileDto, IDomainError>> Handle(GetMyProfileQuery request, CancellationToken cancellationToken)
        {
            // جلب الـ ID حصرياً من التوكن الخاص بالمستخدم الحالي
            var currentUserId = _currentUserService.UserId;
            if (currentUserId == null)
            {
                return DomainError.Unauthorized(); // خطأ عدم المصادقة
            }

            // الاستعلام من قاعدة البيانات باستخدام معرف المستخدم الحالي فقط
            var user = await _userRepository.GetByIdAsync(new Id<User>(currentUserId.Value), cancellationToken);
            if (user == null)
            {
                return DomainError.NotFound();
            }

            // إرجاع البيانات الخاصة به فقط
            return new UserProfileDto(user.Id.Value, user.Username, user.Email);
        }
    }

}