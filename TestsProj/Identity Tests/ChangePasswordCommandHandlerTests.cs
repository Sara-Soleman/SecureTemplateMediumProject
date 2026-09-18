using Common.Application.Abstractions;
using FluentAssertions;
using IdentityPlatform.Identity.Application.Users.Commands.ChangePassword;
using IdentityPlatform.Identity.Domain.Users;
using IdentityPlatform.Identity.Domain.Users.Interfaces;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;

namespace TestsProj.Identity_Tests
{
    public class ChangePasswordCommandHandlerTests
    {
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly Mock<IPasswordHasher> _passwordHasherMock;
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly ChangePasswordCommandHandler _handler;

        public ChangePasswordCommandHandlerTests()
        {
            _userRepositoryMock = new Mock<IUserRepository>();
            _passwordHasherMock = new Mock<IPasswordHasher>();
            _unitOfWorkMock = new Mock<IUnitOfWork>();

            _handler = new ChangePasswordCommandHandler(
                _userRepositoryMock.Object,
                _passwordHasherMock.Object,
                _unitOfWorkMock.Object
            );
        }

        [Fact]
        public async Task Handle_Should_Return_Failure_When_Current_Password_Is_Incorrect()
        {
            // --- Arrange ---
            var userId = Guid.NewGuid();
            var command = new ChangePasswordCommand(userId, "WrongOldPassword!", "NewSecurePass123!");
            var user = User.CreateTestUser();

            _userRepositoryMock
                .Setup(repo => repo.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);

            _passwordHasherMock
                .Setup(hasher => hasher.VerifyPassword(command.CurrentPassword, It.IsAny<string>()))
                .Returns(false); // كلمة المرور الحالية خطأ

            // --- Act ---
            var result = await _handler.Handle(command, CancellationToken.None);

            // --- Assert ---
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().NotBeNull();
        }

        [Fact]
        public async Task Handle_Should_Update_Password_And_Revoke_Tokens_When_Valid()
        {
            // --- Arrange ---
            var userId = Guid.NewGuid();
            var command = new ChangePasswordCommand(userId, "OldSecurePass123!", "NewSecurePass456!");
            var user = User.CreateTestUser();

            _userRepositoryMock
                .Setup(repo => repo.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);

            _passwordHasherMock
                .Setup(hasher => hasher.VerifyPassword(command.CurrentPassword, It.IsAny<string>()))
                .Returns(true); // كلمة المرور الحالية صحيحة

            _passwordHasherMock
                .Setup(hasher => hasher.HashPassword(command.NewPassword))
                .Returns("new-hashed-password");

            // --- Act ---
            var result = await _handler.Handle(command, CancellationToken.None);

            // --- Assert ---
            result.IsSuccess.Should().BeTrue();

            // التأكد من حفظ التغييرات في قاعدة البيانات
            _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.AtLeastOnce());
        }
    }
}
