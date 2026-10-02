using Common.Application.Abstractions;
using Common.Application.Abstractions.DomainEvents;
using FluentAssertions;
using IdentityPlatform.Identity.Application.MFA.SetupMfa;
using IdentityPlatform.Identity.Domain.Users;
using IdentityPlatform.Identity.Domain.Users.Interfaces;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;

namespace TestsProj.Identity_Tests
{
    public class SetupMfaCommandHandlerTests
    {
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly Mock<ITotpService> _totpServiceMock;
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly SetupMfaCommandHandler _handler;
        private readonly Mock<IDomainEventDispatcher> _domainEventDispatcherMock;
        public SetupMfaCommandHandlerTests()
        {
            _userRepositoryMock = new Mock<IUserRepository>();
            _totpServiceMock = new Mock<ITotpService>();
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _domainEventDispatcherMock = new Mock<IDomainEventDispatcher>();


            _handler = new SetupMfaCommandHandler(
                _userRepositoryMock.Object,
                _totpServiceMock.Object,
                _unitOfWorkMock.Object,
                _domainEventDispatcherMock.Object
            );
        }

        [Fact]
        public async Task Handle_Should_Generate_And_Return_Secret_When_User_Exists()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var user = User.CreateTestUser();
            var generatedSecret = "JBSWY3DPEHPK3PXP";

            var command = new SetupMfaCommand(userId);

            _userRepositoryMock
                .Setup(repo => repo.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);

            _totpServiceMock
                .Setup(service => service.GenerateSecretKey())
                .Returns(generatedSecret);

            _unitOfWorkMock
                .Setup(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull();
            result.Value.Secret.Should().Be(generatedSecret);

            _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
