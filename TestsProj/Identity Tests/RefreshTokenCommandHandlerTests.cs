using Common.Application.Abstractions;
using Common.Application.Abstractions.DomainEvents;
using FluentAssertions;
using IdentityPlatform.Identity.Application.Users.Commands.RefreshToken;
using IdentityPlatform.Identity.Domain.Tokens;
using IdentityPlatform.Identity.Domain.Users.Interfaces;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;
namespace TestsProj
{
    public class RefreshTokenCommandHandlerTests
    {
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly Mock<IJwtTokenGenerator> _jwtTokenGeneratorMock;
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly RefreshTokenCommandHandler _handler;
        private readonly Mock<IDomainEventDispatcher> _domainEventDispatcherMock;
        public RefreshTokenCommandHandlerTests()
        {
            // 1. إنشاء "نسخ وهمية" (Mocks) للخدمات التي يعتمد عليها الـ Handler
            _userRepositoryMock = new Mock<IUserRepository>();
            _jwtTokenGeneratorMock = new Mock<IJwtTokenGenerator>();
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _domainEventDispatcherMock = new Mock<IDomainEventDispatcher>();


            // 2. حقن النسخ الوهمية داخل الـ Handler المراد اختباره
            _handler = new RefreshTokenCommandHandler(
                _userRepositoryMock.Object,
                _jwtTokenGeneratorMock.Object,
                _unitOfWorkMock.Object,
                _domainEventDispatcherMock.Object
            );
        }

        [Fact] // تدل على أن هذه الدالة هي عبارة عن اختبار وحدة مستقل
        public async Task Handle_Should_Return_Failure_When_RefreshToken_Is_Null_Or_NotFound()
        {
            // Arrange (التهيئة والاستعداد): تجهيز المعطيات الوهمية للاختبار
            var command = new RefreshTokenCommand("invalid-token-string", "127.0.0.1","TestAgent");

            // نبرمج الـ Repository الوهمي ليقول: "لو تم البحث عن هذا التوكن، أرجع null"
            _userRepositoryMock
                .Setup(repo => repo.GetRefreshTokenByHashAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((RefreshToken?)null);

            // Act (التنفيذ): تشغيل الكود الحقيقي المراد فحصه
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert (التحقق): فحص هل النتيجة تطابق توقعاتنا الأمنية؟
            result.IsSuccess.Should().BeFalse(); // نتوقع أن تفشل العملية
            result.Error.Should().NotBeNull();  // نتوقع وجود خطأ
        }
    }
}
