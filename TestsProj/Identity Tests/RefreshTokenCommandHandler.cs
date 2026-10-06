using Common.Application.Abstractions;
using Common.Application.Abstractions.DomainEvents;
using Common.Domain;
using Common.Domain.Errors;
using FluentAssertions;
using IdentityPlatform.Authorization.Domain.Roles.Interfaces;
using IdentityPlatform.Identity.Application.Users.Commands.RefreshToken;
using IdentityPlatform.Identity.Domain.Tokens;
using IdentityPlatform.Identity.Domain.Users.Interfaces;
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
        private readonly Mock<IRoleRepository> _roleRepositoryMock;
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly RefreshTokenCommandHandler _handler;
        private readonly Mock<IDomainEventDispatcher> _domainEventDispatcherMock;

        public RefreshTokenCommandHandlerTests()
        {
            _userRepositoryMock = new Mock<IUserRepository>();
            _jwtTokenGeneratorMock = new Mock<IJwtTokenGenerator>();
            _roleRepositoryMock = new Mock<IRoleRepository>();
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _domainEventDispatcherMock = new Mock<IDomainEventDispatcher>();

            _handler = new RefreshTokenCommandHandler(_userRepositoryMock.Object, _jwtTokenGeneratorMock.Object, _roleRepositoryMock.Object, _unitOfWorkMock.Object, _domainEventDispatcherMock.Object);
        }

        [Fact]
        public async Task Handle_WhenTokenAlreadyConsumed_ShouldRevokeFamilyAndReturnFailure()
        {
            // Arrange
            var rawToken = "some_refresh_token";
            var command = new RefreshTokenCommand(rawToken, "127.0.0.1", "TestAgent");

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

            storedToken.SetFamily(family);
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

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Should().BeEquivalentTo(DomainError.SecurityAlertTokenReuseDetected());
            family.RevokedAt.Should().NotBeNull(); // التأكد من أنه تم إلغاء العائلة أمنياً
        }
    }
}
