using Common.Application.Abstractions;
using Common.Application.Abstractions.DomainEvents;
using Common.Application.Interfaces;
using FluentAssertions;
using IdentityPlatform.Identity.Application.Users.Commands.Login;
using IdentityPlatform.Identity.Domain.Users;
using IdentityPlatform.Identity.Domain.Users.Enums;
using IdentityPlatform.Identity.Domain.Users.Interfaces;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;

namespace TestsProj.Identity_Tests
{
    public class LoginCommandHandlerTests
    {
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly Mock<IPasswordHasher> _passwordHasherMock;
        private readonly Mock<IEmailService> _emailServiceMock;
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly LoginCommandHandler _handler;
        private readonly Mock<IDomainEventDispatcher> _domainEventDispatcherMock;

        public LoginCommandHandlerTests()
        {
            _userRepositoryMock = new Mock<IUserRepository>();
            _passwordHasherMock = new Mock<IPasswordHasher>();
            _emailServiceMock = new Mock<IEmailService>();
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _domainEventDispatcherMock = new Mock<IDomainEventDispatcher>();


            _handler = new LoginCommandHandler(
                _userRepositoryMock.Object,
                _passwordHasherMock.Object,
                _emailServiceMock.Object,
                _unitOfWorkMock.Object,
                _domainEventDispatcherMock.Object
            );
        }

        [Fact]
        public async Task Handle_Should_Return_Failure_When_User_Not_Found()
        {
            // Arrange
            var command = new LoginCommand("wrong@example.com", "Password123!");

            _userRepositoryMock
                .Setup(repo => repo.GetByUsernameOrEmailAsync(command.UsernameOrEmail, It.IsAny<CancellationToken>()))
                .ReturnsAsync((User?)null);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().NotBeNull();
        }

        [Fact]
        public async Task Handle_Should_Return_Failure_When_Password_Is_Invalid()
        {
            // Arrange
            var email = "sara@example.com";
            var wrongPassword = "WrongPassword123!";
            var user = User.CreateTestUser(); // تأكد من توفر هذه الدالة أو إنشاء كائن User بالطريقة المناسبة لديك

            var command = new LoginCommand(email, wrongPassword);

            _userRepositoryMock
                .Setup(repo => repo.GetByUsernameOrEmailAsync(email, It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);

            _passwordHasherMock
                .Setup(hasher => hasher.VerifyPassword(wrongPassword, It.IsAny<string>()))
                .Returns(false);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().NotBeNull();
        }

        [Fact]
        public async Task Handle_Should_Return_MfaChallenge_Email_When_Credentials_Valid_And_Preference_Is_Email()
        {
            // Arrange
            var email = "sara@example.com";
            var password = "SecurePassword123!";
            var user = User.CreateTestUser();

            // فرض أن القناة المفضلة للمستخدم هي البريد الإلكتروني
            
            user.ChangePreferredMfaType(MfaType.Email);

            var command = new LoginCommand(email, password);

            _userRepositoryMock
                .Setup(repo => repo.GetByUsernameOrEmailAsync(email, It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);

            _passwordHasherMock
                .Setup(hasher => hasher.VerifyPassword(password, It.IsAny<string>()))
                .Returns(true);

            _unitOfWorkMock
                .Setup(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            _emailServiceMock
                .Setup(service => service.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull();
            result.Value.MfaType.Should().Be("Email");

            // التأكد من استدعاء خدمة البريد وحفظ التغييرات
            _emailServiceMock.Verify(service => service.SendEmailAsync(user.Email, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
            _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_Should_Return_MfaChallenge_Totp_When_Credentials_Valid_And_Preference_Is_Totp()
        {
            // Arrange
            var email = "sara@example.com";
            var password = "SecurePassword123!";
            var user = User.CreateTestUser();

            // [مهم جداً]: يجب إضافة مفتاح سري وهمي أولاً لكي يسمح الكلاس بالتحويل إلى TOTP
            user.SetupMfaSecret("JBSWY3DPEHPK3PXP");
            user.ChangePreferredMfaType(MfaType.Totp);

            var command = new LoginCommand(email, password);

            _userRepositoryMock
                .Setup(repo => repo.GetByUsernameOrEmailAsync(email, It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);

            _passwordHasherMock
                .Setup(hasher => hasher.VerifyPassword(password, It.IsAny<string>()))
                .Returns(true);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull();
            result.Value.MfaType.Should().Be("Totp");

            _emailServiceMock.Verify(service => service.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}
