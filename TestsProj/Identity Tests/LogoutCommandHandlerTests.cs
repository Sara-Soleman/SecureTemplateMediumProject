using Common.Application.Abstractions;
using Common.Domain;
using FluentAssertions;
using IdentityPlatform.Identity.Application.Users.Commands.Logout;
using IdentityPlatform.Identity.Domain.Tokens;
using IdentityPlatform.Identity.Domain.Users;
using IdentityPlatform.Identity.Domain.Users.Interfaces;

using Moq;
using System;
using System.Collections.Generic;
using System.Text;

namespace TestsProj.Identity_Tests
{
    public class LogoutCommandHandlerTests
    {
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly LogoutCommandHandler _handler;

        public LogoutCommandHandlerTests()
        {
            _userRepositoryMock = new Mock<IUserRepository>();
            _unitOfWorkMock = new Mock<IUnitOfWork>();

            _handler = new LogoutCommandHandler(
                _userRepositoryMock.Object,
                _unitOfWorkMock.Object
            );
        }

        [Fact]
        public async Task Handle_Should_Return_Success_Even_When_Token_Not_Found_For_Security()
        {
            // --- Arrange ---
            var refreshToken = "some-random-refresh-token";
            var command = new LogoutCommand(refreshToken);

            // المحاكي يعيد null (التوكن غير موجود)
            _userRepositoryMock
                .Setup(repo => repo.GetRefreshTokenByHashAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((RefreshToken?)null);

            // --- Act ---
            var result = await _handler.Handle(command, CancellationToken.None);

            // --- Assert ---
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().BeTrue();

            // التأكد من عدم استدعاء الحفظ لأن شيئاً لم يتغير
            _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());
        }

        [Fact]
        public async Task Handle_Should_Revoke_Token_Family_And_Save_When_Token_Exists()
        {
            // --- Arrange ---
            var refreshTokenStr = "valid-refresh-token";
            var command = new LogoutCommand(refreshTokenStr);

            // استخدام دوال الـ Create الحقيقية الموجودة في كلاساتك
            var userId = Id<User>.New();
            var family = RefreshTokenFamily.Create(userId, Guid.NewGuid(), "127.0.0.1", "Mozilla", TimeSpan.FromDays(7));
            var token = RefreshToken.Create(family.Id, "hashed-token", TimeSpan.FromDays(7));

            // ربط التوكن بالعائلة لضمان عمل الخاصية Navigation Property
            token.SetFamily(family);

            _userRepositoryMock
                .Setup(repo => repo.GetRefreshTokenByHashAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(token);

            // --- Act ---
            var result = await _handler.Handle(command, CancellationToken.None);

            // --- Assert ---
            result.IsSuccess.Should().BeTrue();

            // التأكد من أن عائلة التوكن تم إلغاؤها وحفظ التغييرات
            family.RevokedAt.Should().NotBeNull();
            _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.AtLeastOnce());
        }
    }
}
