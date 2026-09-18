using Common.Application.Abstractions;
using FluentAssertions;
using IdentityPlatform.Identity.Application.Users.Commands.RegisterUser;
using IdentityPlatform.Identity.Domain.Users;
using IdentityPlatform.Identity.Domain.Users.Interfaces;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;

namespace TestsProj.Identity_Tests
{
    public class RegisterCommandHandlerTests
    {
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly Mock<IPasswordHasher> _passwordHasherMock;
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly RegisterUserCommandHandler _handler;

        public RegisterCommandHandlerTests()
        {
            _userRepositoryMock = new Mock<IUserRepository>();
            _passwordHasherMock = new Mock<IPasswordHasher>();
            _unitOfWorkMock = new Mock<IUnitOfWork>();

            _handler = new RegisterUserCommandHandler(
                _userRepositoryMock.Object,
                _passwordHasherMock.Object,
                _unitOfWorkMock.Object
            );
        }

        [Fact]
        public async Task Handle_Should_Return_Failure_When_Email_Already_Exists()
        {
            // --- Arrange ---
            var command = new RegisterUserCommand("sara@example.com", "SecurePassword123!", "Sara");
            //var existingUser = User.CreateTestUser();
           
            var existingUser = User.Create("ExistingUser", command.Email, "some-valid-hash");
            
            _userRepositoryMock
        .Setup(repo => repo.ExistsByUsernameOrEmailAsync(command.Username, command.Email, It.IsAny<CancellationToken>()))
        .ReturnsAsync(true);
            // --- Act ---
            var result = await _handler.Handle(command, CancellationToken.None);

            // --- Assert ---
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().NotBeNull();
        }

        [Fact]
        public async Task Handle_Should_Register_User_Successfully_When_Request_Is_Valid()
        {
            // --- Arrange ---
            var command = new RegisterUserCommand("sara@example.com", "SecurePassword123!", "Sara");

            _userRepositoryMock
                .Setup(repo => repo.GetByUsernameOrEmailAsync(command.Email, It.IsAny<CancellationToken>()))
                .ReturnsAsync((User?)null); // البريد غير موجود (متاح)

            _passwordHasherMock
                .Setup(hasher => hasher.HashPassword(command.Password))
                .Returns("hashed-secure-password");

            // --- Act ---
            var result = await _handler.Handle(command, CancellationToken.None);

            // --- Assert ---
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeEmpty();

            // التأكد من استدعاء حفظ التغييرات في قاعدة البيانات
            _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.AtLeastOnce());
        }
    }
}
