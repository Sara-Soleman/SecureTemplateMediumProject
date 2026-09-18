using Common.Application.Abstractions;
using Common.Domain;
using FluentAssertions;
using IdentityPlatform.Identity.Application.Sessions.RevokeAllUserSessions;
using IdentityPlatform.Identity.Domain.Sessions;
using IdentityPlatform.Identity.Domain.Sessions.Interfaces;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;

namespace TestsProj.Identity_Tests
{
    public class RevokeAllUserSessionsCommandHandlerTests
    {
        private readonly Mock<IUserSessionRepository> _sessionRepositoryMock;
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly RevokeAllUserSessionsCommandHandler _handler;

        public RevokeAllUserSessionsCommandHandlerTests()
        {
            _sessionRepositoryMock = new Mock<IUserSessionRepository>();
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _handler = new RevokeAllUserSessionsCommandHandler(_sessionRepositoryMock.Object, _unitOfWorkMock.Object);
        }

        [Fact]
        public async Task Handle_WithValidUserId_ShouldRevokeAllActiveSessionsAndSave()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var command = new RevokeAllUserSessionsCommand(userId);

            // إنشاء جلسات وهمية نشطة للمستخدم
            var activeSessions = new List<UserSession>
        {
            UserSession.Create( userId, "token1","127.0.0.1", "Agent1", DateTimeOffset.UtcNow.AddDays(1)),
            UserSession.Create( userId, "token2", "127.0.0.1", "Agent2", DateTimeOffset.UtcNow.AddDays(1))
        };



            _sessionRepositoryMock
                .Setup(r => r.GetActiveSessionsByUserIdAsync(userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(activeSessions);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.IsSuccess.Should().BeTrue();

            // التأكد من أنه تم استدعاء التحديث وحفظ التغييرات
            _sessionRepositoryMock.Verify(r => r.Update(It.IsAny<UserSession>()), Times.Exactly(2));
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once());
        }
    }
}
