using Common.Application.Abstractions;
using Common.Application.Abstractions.DomainEvents;
using FluentAssertions;
using IdentityPlatform.Identity.Application.MFA.ChangeMfaType;
using IdentityPlatform.Identity.Application.Persistence;
using IdentityPlatform.Identity.Domain.Users;
using IdentityPlatform.Identity.Domain.Users.Enums;
using IdentityPlatform.Identity.Domain.Users.Interfaces;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace TestsProj.Identity_Tests
{
    public class ChangeMfaPreferenceCommandHandlerTests
    {
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly Mock<IIdentityUnitOfWork> _unitOfWorkMock;
        private readonly ChangeMfaPreferenceCommandHandler _handler;

        private readonly Mock<IDomainEventDispatcher> _domainEventDispatcherMock;

        public ChangeMfaPreferenceCommandHandlerTests()
        {
            _userRepositoryMock = new Mock<IUserRepository>();
            _unitOfWorkMock = new Mock<IIdentityUnitOfWork>();
            _domainEventDispatcherMock = new Mock<IDomainEventDispatcher>();
            _handler = new ChangeMfaPreferenceCommandHandler(_userRepositoryMock.Object, _unitOfWorkMock.Object, _domainEventDispatcherMock.Object);
        }

        [Fact]
        public async Task Handle_Should_ReturnSuccess_WhenUserExistsAndPreferenceIsUpdated()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var user = User.CreateTestUser(userId.ToString()); // دالة افتراضية لإنشاء مستخدم تجريبي أو تمرير الكائن الفعلي

            _userRepositoryMock
                .Setup(repo => repo.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);

            _unitOfWorkMock
                .Setup(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            var command = new ChangeMfaPreferenceCommand(userId, MfaType.Email);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_Should_ReturnFailure_WhenUserNotFound()
        {
            // Arrange
            var userId = Guid.NewGuid();
            _userRepositoryMock
                .Setup(repo => repo.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((User)null!);

            var command = new ChangeMfaPreferenceCommand(userId, MfaType.Totp);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsFailure.Should().BeTrue();
            _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }
        [Fact]
        public async Task Handle_Should_Return_Success_When_User_Changes_Mfa_Preference()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var user = User.CreateTestUser();

            _userRepositoryMock
                .Setup(repo => repo.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);

            _unitOfWorkMock
                .Setup(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            var command = new ChangeMfaPreferenceCommand(userId, MfaType.Email);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
