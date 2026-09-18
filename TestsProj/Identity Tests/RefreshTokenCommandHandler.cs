using Common.Application.Abstractions;
using Common.Domain;
using FluentAssertions;
using IdentityPlatform.Identity.Application.Users.Commands.RefreshToken;
using IdentityPlatform.Identity.Domain.Tokens;
using IdentityPlatform.Identity.Domain.Users.Interfaces;
using IdentityPlatform.Identity.Infrastructure.Services;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;

namespace TestsProj.Identity_Tests
{
    public class RefreshTokenCommandHandlerTests
    {
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly Mock<IJwtTokenGenerator> _jwtTokenGeneratorMock;
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly RefreshTokenCommandHandler _handler;

        public RefreshTokenCommandHandlerTests()
        {
            _userRepositoryMock = new Mock<IUserRepository>();
            _jwtTokenGeneratorMock = new Mock<IJwtTokenGenerator>();
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _handler = new RefreshTokenCommandHandler(_userRepositoryMock.Object, _jwtTokenGeneratorMock.Object, _unitOfWorkMock.Object);
        }

        [Fact]
        public async Task Handle_WhenTokenAlreadyConsumed_ShouldRevokeFamilyAndReturnFailure()
        {
            // Arrange
            var rawToken = "some_refresh_token";
            var command = new RefreshTokenCommand(rawToken, "127.0.0.1");

            var familyId = Id<RefreshTokenFamily>.New();
            var userId = Guid.NewGuid();
            var sessionId = Guid.NewGuid();

            // إنشاء عائلة وهمية
            var family = RefreshTokenFamily.Create(
                id: familyId,
                userId: userId,
                sessionId: sessionId,
                ipAddress: "127.0.0.1",
                userAgent: "TestAgent",
                lifetime: TimeSpan.FromDays(7)
            );

            // إنشاء التوكن
            var storedToken = RefreshToken.Create(
                familyId: familyId,
                tokenHash: TokenSecurityHelper.HashToken(rawToken),
                lifetime: TimeSpan.FromDays(7),
                id: Id<RefreshToken>.New()
            );

            // استهلاك التوكن مسبقاً
            storedToken.Consume(Id<RefreshToken>.New());

            // ملاحظة حاسمة: بما أن storedToken.Family هي Navigation Property، ولتجنب الـ NullReference 
            // إذا كانت الـ Entity تعتمد على الـ Navigation Property مباشرة، تأكد كيف يعيدها الـ Repository.
            // إذا كانت الخاصية تمتلك Backing Field أو يمكن الوصول لها، أو إذا استطاعت الـ Entity ربطها.

            // إذا كنت تستخدم Moq للـ Repository ويمكنك جعل الـ StoredToken يعيد الـ Family (إذا كانت الـ Family خاصية virtual):
            // (أو يمكنك استخدام Mock لـ RefreshToken نفسه إذا لزم الأمر، لكن الأفضل ضبط الكيان).

            _userRepositoryMock
                .Setup(r => r.GetRefreshTokenByHashAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(storedToken);

            // إذا كانت الـ Family تُجلب من الـ storedToken.Family وكان الـ storedToken كائناً حقيقياً وليست Interface:
            // تأكد هل الـ Family يتم تعيينها عبر Constructor الـ RefreshToken أم عبر ربط لاحق؟
            // إذا كان بإمكانك تمرير الـ Family أو إذا كان الـ RefreshToken يخزن الـ Family داخلياً، اضبطها بالطريقة التي صممت بها الـ Domain Model.
        }
    }
}
