using Common.Application.Abstractions;
using Common.Application.Abstractions.DomainEvents;
using Common.Domain;
using FluentAssertions;
using IdentityPlatform.Identity.Application.Helpers;
using IdentityPlatform.Identity.Application.Persistence;
using IdentityPlatform.Identity.Application.Users.Commands.RefreshToken;
using IdentityPlatform.Identity.Domain.Tokens;
using IdentityPlatform.Identity.Domain.Users;
using IdentityPlatform.Identity.Domain.Users.Interfaces;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace TestsProj
{
    public class RefreshTokenCommandHandlerSuccessTests
    {
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly Mock<IJwtTokenGenerator> _jwtTokenGeneratorMock;
        private readonly Mock<IIdentityUnitOfWork> _unitOfWorkMock;
        private readonly RefreshTokenCommandHandler _handler;
        private readonly Mock<IDomainEventDispatcher> _domainEventDispatcherMock;
        public RefreshTokenCommandHandlerSuccessTests()
        {
            _userRepositoryMock = new Mock<IUserRepository>();
            _jwtTokenGeneratorMock = new Mock<IJwtTokenGenerator>();
            _unitOfWorkMock = new Mock<IIdentityUnitOfWork>();
            _domainEventDispatcherMock = new Mock<IDomainEventDispatcher>();


            _handler = new RefreshTokenCommandHandler(
                _userRepositoryMock.Object,
                _jwtTokenGeneratorMock.Object,
                _unitOfWorkMock.Object,
                _domainEventDispatcherMock.Object
            );
        }

        [Fact]
        public async Task Handle_Should_Rotate_Token_Successfully_When_Request_Is_Valid()
        {
            // --- 1. Arrange (التهيئة) ---
            var ipAddress = "127.0.0.1";
            var rawToken = "valid-raw-token";
            var tokenHash = TokenSecurityHelper.HashToken(rawToken);

            var userId = Id<User>.New();
            var sessionId = Guid.NewGuid();

            // إنشاء عائلة توكنات وهمية بنفس الـ IP الحالي وبحالة صالحة
            var family = RefreshTokenFamily.Create(userId, sessionId, ipAddress, "Mozilla/5.0", TimeSpan.FromDays(7));

            // إنشاء توكن وهمي نشط وغير مستهلك
            var storedToken = RefreshToken.Create(family.Id, tokenHash, TimeSpan.FromDays(7));
            storedToken.SetFamily(family);
            family.AddRefreshToken(storedToken);

            var user = User.CreateTestUser(); // (افترض وجود طريقة لإنشاء مستخدم وهمي للاختبار أو استخدم الـ Constructor الخاص بك)

            // برمجة الخدمات الوهمية لترجع البيانات السليمة
            _userRepositoryMock
                .Setup(repo => repo.GetRefreshTokenByHashAsync(tokenHash, It.IsAny<CancellationToken>()))
                .ReturnsAsync(storedToken);

            _userRepositoryMock
                .Setup(repo => repo.GetBySessionIdAsync(sessionId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);
           

            _jwtTokenGeneratorMock
                .Setup(gen => gen.GenerateToken(
                    user,
                    sessionId,
                    It.IsAny<IEnumerable<string>>(),
                    It.IsAny<IEnumerable<string>>()))
                .Returns("new-mock-jwt-access-token");

            var command = new RefreshTokenCommand(rawToken, ipAddress, "TestAgent");

            // --- 2. Act (التنفيذ) ---
            var result = await _handler.Handle(command, CancellationToken.None);

            // --- 3. Assert (التحقق) ---
            result.IsSuccess.Should().BeTrue();          // نتوقع أن العملية نجحت
            result.Value.Should().NotBeNull();         // نتوقع وجود استجابة تحتوي التوكنات
            result.Value.AccessToken.Should().Be("new-mock-jwt-access-token"); // التحقق من الـ Access Token الجديد
            result.Value.RefreshToken.Should().NotBeNullOrEmpty(); // التحقق من توليد Refresh Token جديد

            // التأكد من أن التوكن القديم تم استهلاكه (Consumed)
            storedToken.ConsumedAt.Should().NotBeNull();

            // التأكد من أن قاعدة البيانات حفظت التغييرات
            _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.AtLeastOnce());
        }
    }
}