using Common.Application.Abstractions;
using Common.Application.Abstractions.DomainEvents;
using FluentAssertions;
using IdentityPlatform.Identity.Application.Persistence;
using IdentityPlatform.Identity.Application.Users.Commands.Login;
using IdentityPlatform.Identity.Domain.Dto;
using IdentityPlatform.Identity.Domain.Users;
using IdentityPlatform.Identity.Domain.Users.Enums;
using IdentityPlatform.Identity.Domain.Users.Interfaces;
using Microsoft.AspNetCore.Http;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace TestsProj.Identity_Tests
{
    public class VerifyLoginMfaCommandHandlerTests
    {
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly Mock<ITotpService> _totpServiceMock;
        private readonly Mock<IJwtTokenGenerator> _tokenServiceMock; // تم التعديل لتطابق الخدمة المستخدمة في الهاندلر
        private readonly Mock<IIdentityUnitOfWork> _unitOfWorkMock;
        private readonly VerifyLoginMfaCommandHandler _handler;
        private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
        private readonly Mock<IDomainEventDispatcher> _domainEventDispatcherMock;

        public VerifyLoginMfaCommandHandlerTests()
        {
            _userRepositoryMock = new Mock<IUserRepository>();
            _totpServiceMock = new Mock<ITotpService>();
            _tokenServiceMock = new Mock<IJwtTokenGenerator>();
            _unitOfWorkMock = new Mock<IIdentityUnitOfWork>();
            _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            _domainEventDispatcherMock = new Mock<IDomainEventDispatcher>();


            _handler = new VerifyLoginMfaCommandHandler(
                _userRepositoryMock.Object,
                _totpServiceMock.Object,
                _tokenServiceMock.Object,
                _unitOfWorkMock.Object,
                _httpContextAccessorMock.Object,
                _domainEventDispatcherMock.Object
            );
        }

        [Fact]
        public async Task Handle_Should_Return_Failure_When_User_Not_Found()
        {
            var userId = Guid.NewGuid();
            var command = new VerifyLoginMfaCommand(userId, "123456");

            _userRepositoryMock
                .Setup(repo => repo.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((User?)null);

            var result = await _handler.Handle(command, CancellationToken.None);

            result.IsSuccess.Should().BeFalse();
            result.Error.Should().NotBeNull();
        }

        [Fact]
        public async Task Handle_Should_Return_Failure_When_Email_Otp_Is_Invalid()
        {
            var userId = Guid.NewGuid();
            var user = User.CreateTestUser();
            user.ChangePreferredMfaType(MfaType.Email);

            user.SetEmailOtp("123456", TimeSpan.FromMinutes(5));

            var command = new VerifyLoginMfaCommand(userId, "999999");

            _userRepositoryMock
                .Setup(repo => repo.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);

            var result = await _handler.Handle(command, CancellationToken.None);

            result.IsSuccess.Should().BeFalse();
            result.Error.Should().NotBeNull();
        }

        [Fact]
        public async Task Handle_Should_Return_Success_And_Token_When_Email_Otp_Is_Valid()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var user = User.CreateTestUser();
            user.ChangePreferredMfaType(MfaType.Email);

            var validCode = "123456";
            user.SetEmailOtp(validCode, TimeSpan.FromMinutes(5));

            var command = new VerifyLoginMfaCommand(userId, validCode);

            _userRepositoryMock
                .Setup(repo => repo.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);

            _unitOfWorkMock
                .Setup(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            // تجهيز الـ DTO المتوقع إرجاعه من خدمة التوكنات
            var expectedAuthResponse = new AuthenticationResponseDto(
                AccessToken: "mock-access-token",
                RefreshToken: "mock-refresh-token",
                ExpiresAt: DateTimeOffset.UtcNow.AddHours(1)
            );

            
            _tokenServiceMock
                    .Setup(service => service.GenerateTokensAsync(
                        user,
                        It.IsAny<string>(),
                        It.IsAny<string>(),
                        It.IsAny<CancellationToken>()))
                    .ReturnsAsync(expectedAuthResponse);
            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull();
            result.Value.AccessToken.Should().Be("mock-access-token");
            result.Value.RefreshToken.Should().Be("mock-refresh-token");

            _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
    }

}
