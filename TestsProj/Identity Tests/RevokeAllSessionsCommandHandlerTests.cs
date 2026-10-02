using Common.Application.Abstractions;
using Common.Application.Abstractions.DomainEvents;
using FluentAssertions;
using IdentityPlatform.Identity.Application.Users.Commands.RevokeAllSessions;
using IdentityPlatform.Identity.Domain.Users;
using IdentityPlatform.Identity.Domain.Users.Interfaces;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;

namespace TestsProj.Identity_Tests
{
    public class RevokeAllSessionsCommandHandlerTests
    {
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly RevokeAllSessionsCommandHandler _handler;
        private readonly Mock<IDomainEventDispatcher> _domainEventDispatcherMock;

        public RevokeAllSessionsCommandHandlerTests()
        {
            _userRepositoryMock = new Mock<IUserRepository>();
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _domainEventDispatcherMock = new Mock<IDomainEventDispatcher>();


            _handler = new RevokeAllSessionsCommandHandler(
                _userRepositoryMock.Object,
                _unitOfWorkMock.Object,
                _domainEventDispatcherMock.Object
            );
        }

        [Fact]
        public async Task Handle_Should_Return_Failure_When_User_Not_Found()
        {
            // --- Arrange ---
            var userId = Guid.NewGuid();
            var command = new RevokeAllSessionsCommand(userId);

            _userRepositoryMock
                .Setup(repo => repo.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((User?)null);

            // --- Act ---
            var result = await _handler.Handle(command, CancellationToken.None);

            // --- Assert ---
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().NotBeNull();
        }

        [Fact]
        public async Task Handle_Should_Increment_Token_Version_And_Save_When_User_Exists()
        {
            // --- Arrange ---
            var userId = Guid.NewGuid();
            var command = new RevokeAllSessionsCommand(userId);
            var user = User.CreateTestUser();
            var initialVersion = user.TokenVersion;

            _userRepositoryMock
                .Setup(repo => repo.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);

            // --- Act ---
            var result = await _handler.Handle(command, CancellationToken.None);

            // --- Assert ---
            result.IsSuccess.Should().BeTrue();
            user.TokenVersion.Should().Be(initialVersion + 1); // التأكد من زيادة رقم الإصدار لإبطال الجلسات

            _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.AtLeastOnce());
        }
    }
}
