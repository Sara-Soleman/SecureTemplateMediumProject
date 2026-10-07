using Common.Application.Abstractions.DomainEvents;
using Common.Application.Interfaces;
using Common.Domain.Errors;
using FluentAssertions;
using IdentityPlatform.Authorization.Application.Persistence;
using IdentityPlatform.Authorization.Application.Resource_Based_Authorization;
using IdentityPlatform.Authorization.Application.Roles.Commands.UpdateRoles;
using IdentityPlatform.Authorization.Application.Roles.Dtos;
using IdentityPlatform.Authorization.Domain.Roles;
using IdentityPlatform.Authorization.Domain.Roles.Interfaces;
using IdentityPlatform.Authorization.Infrastructure.Persistence;
using IdentityPlatform.Identity.Application.MFA.VerifyAndEnableMfa;
using IdentityPlatform.Identity.Application.Persistence;
using IdentityPlatform.Identity.Application.Users.Commands.Login;
using IdentityPlatform.Identity.Application.Users.Commands.RefreshToken;
using IdentityPlatform.Identity.Application.Users.Commands.RegisterUser;
using IdentityPlatform.Identity.Domain.Dto;
using IdentityPlatform.Identity.Domain.Users;
using IdentityPlatform.Identity.Domain.Users.Interfaces;
using IdentityPlatform.Identity.Infrastructure.Persistence;
using IdentityPlatform.Identity.Infrastructure.Repositories;
using IdentityPlatform.Identity.Infrastructure.Services;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
using Xunit;

namespace TestsProj.AuthorizationProj
{
    public class UpdateRoleCommandHandlerTests
    {
        private readonly Mock<IAuthorizationService> _authorizationServiceMock;
        private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
        private readonly Mock<IRoleRepository> _roleRepositoryMock;
        private readonly UpdateRoleCommandHandler _handler;

        private readonly Mock<IAuthorizationUnitOfWork> _unitOfWorkMock;         
        private readonly Mock<IDomainEventDispatcher> _domainEventDispatcherMock;
        public UpdateRoleCommandHandlerTests()
        {
            _authorizationServiceMock = new Mock<IAuthorizationService>();
            _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            _roleRepositoryMock = new Mock<IRoleRepository>();
            _unitOfWorkMock = new Mock<IAuthorizationUnitOfWork>();                 
            _domainEventDispatcherMock = new Mock<IDomainEventDispatcher>();

            _handler = new UpdateRoleCommandHandler(
                _authorizationServiceMock.Object,
                _httpContextAccessorMock.Object,
                _roleRepositoryMock.Object,
                _unitOfWorkMock.Object,
            _domainEventDispatcherMock.Object
            );
        }
        [Fact]
        public async Task Complete_User_Journey_Direct_Handler_Test()
        {
            // 1. إعداد الـ DI Container الخاص بالاختبار
            var services = new ServiceCollection();

            services.AddLogging();

            // قواعد بيانات منفصلة في الذاكرة
            services.AddDbContext<IdentityDbContext>(options => options.UseInMemoryDatabase(Guid.NewGuid().ToString()));
            services.AddDbContext<AuthorizationDbContext>(options => options.UseInMemoryDatabase(Guid.NewGuid().ToString()));

            // تسجيل الخدمات والـ Repositories والـ Mocks المطلوبة
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IIdentityUnitOfWork, IdentityUnitOfWork>();
            services.AddScoped<IAuthorizationUnitOfWork, AuthorizationUnitOfWork>();
            services.AddTransient<IDomainEventDispatcher>(_ => Mock.Of<IDomainEventDispatcher>());

            // **التصحيح الأول:** استخدام التطبيق الحقيقي أو Mock صحيح للـ PasswordHasher
            services.AddScoped<IPasswordHasher, PasswordHasher>();

            services.AddScoped<IEmailService>(_ => Mock.Of<IEmailService>());

           
            services.AddScoped<ITotpService, AlwaysValidTotpService>();
            // أضف هذا السطر مع بقية تسجيل الخدمات في بداية الاختبار إذا لم يكن موجوداً:
            services.AddScoped<IJwtTokenGenerator>(_ => {
                var mock = new Mock<IJwtTokenGenerator>();
                mock.Setup(g => g.GenerateTokensAsync(It.IsAny<User>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(new AuthenticationResponseDto("mock-access-token", "mock-refresh-token", DateTime.Now.AddHours(1)));
                return mock.Object;
            });
            services.AddHttpContextAccessor();

            // تسجيل الـ MediatR
            services.AddMediatR(cfg => {
                cfg.RegisterServicesFromAssembly(typeof(RegisterUserCommand).Assembly);
            });

            var serviceProvider = services.BuildServiceProvider();
            var mediator = serviceProvider.GetRequiredService<IMediator>();

            // =========================================================================
            // الخطوة 1: تسجيل مستخدم جديد (Registration - Happy Path)
            // =========================================================================
            var registerCommand = new RegisterUserCommand("User", "integration.user@test.com", "SecurePassword123!");
            var registerResult = await mediator.Send(registerCommand);
            registerResult.Should().NotBeNull();


            // =========================================================================
            // الخطوة 2: محاولة تسجيل الدخول (Login - يؤدي إلى إرسال MFA Challenge)
            // =========================================================================
            var loginCommand = new LoginCommand("integration.user@test.com", "SecurePassword123!");
            var loginResult = await mediator.Send(loginCommand);

            loginResult.Should().NotBeNull();
            loginResult.IsSuccess.Should().BeTrue();
            loginResult.Value.Should().NotBeNull();
            var userId = loginResult.Value.UserId;


            // =========================================================================
            // الخطوة 3: التحقق من الـ MFA للحصول على التوكنات النهائية (Verify MFA)
            // =========================================================================
            // The login handler generated and stored an email OTP. Retrieve it from the DB so the test uses the actual code.
            // The login handler stored the email OTP in the repository. Use the IUserRepository to fetch the user and read the OTP.
            var userRepo = serviceProvider.GetRequiredService<IUserRepository>();
            var userEntity = await userRepo.GetByIdAsync(userId, CancellationToken.None);
            var otpCode = userEntity?.EmailOtpCode ?? "123456";
            var verifyMfaCommand = new VerifyLoginMfaCommand(userId, otpCode);
            var authResult = await mediator.Send(verifyMfaCommand);

            

            // اطبع تفاصيل الخطأ هنا لمعرفة هل المشكلة في الـ MfaSecret الفارغ أم في الـ Code نفسه:
            if (!authResult.IsSuccess)
            {
                System.Diagnostics.Debug.WriteLine($"==== ERROR TYPE: {authResult.Error?.ErrorType}, MESSAGE: {authResult.Error?.ErrorMessage} ====");
            }

            authResult.IsSuccess.Should().BeTrue();
            authResult.IsSuccess.Should().BeTrue();
            authResult.Should().NotBeNull();
            authResult.IsSuccess.Should().BeTrue();
            authResult.Value.AccessToken.Should().NotBeNullOrEmpty();
            authResult.Value.RefreshToken.Should().NotBeNullOrEmpty();

            var accessToken = authResult.Value.AccessToken;
            var refreshToken = authResult.Value.RefreshToken;


            // =========================================================================
            // الخطوة 4: تجديد الـ Refresh Token (Token Refresh Lifecycle)
            // =========================================================================
            var refreshTokenCommand = new RefreshTokenCommand(refreshToken, "127.0.0.1", "TestAgent");
            var refreshResult = await mediator.Send(refreshTokenCommand);

            refreshResult.Should().NotBeNull();
            refreshResult.IsSuccess.Should().BeTrue();
            refreshResult.Value.AccessToken.Should().NotBeNullOrEmpty();
            refreshResult.Value.RefreshToken.Should().NotBeNullOrEmpty();
        }
        [Fact]
        public async Task Handle_WhenUserTriesToModifySystemRole_ShouldReturnUnauthorizedResult()
        {
            // --- Arrange (التحضير) ---
            var roleId = Guid.NewGuid();
            var command = new UpdateRoleCommand(roleId, "New Name", "New Description", new List<string> { "Perm.1" });

            // دور من نوع System Role (لا يمكن تعديله)
            var permissionsList = new List<string> { "Permissions.Roles.View" };
            var role = Role.Create("Admin", "System Admin Role", permissionsList);

            _roleRepositoryMock
                .Setup(repo => repo.GetByIdAsync(roleId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(role);

            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "user-123") }, "TestAuth"));
            _httpContextAccessorMock
                .Setup(x => x.HttpContext)
                .Returns(new DefaultHttpContext { User = claimsPrincipal });

            // 💡 الحل الصحيح لتجنب خطأ الـ Extension Method في Moq:
            // بما أن الـ Handler يستدعي AuthorizeAsync مع مصفوفة متطلبات (IEnumerable<IAuthorizationRequirement>)،
            // سنقوم بعمل Setup للدالة التي تقبل array/collection صراحة عبر الـ Interface مباشرة.
            _authorizationServiceMock 
                .Setup(x => x.AuthorizeAsync(
                    It.IsAny<ClaimsPrincipal>(),
                    It.IsAny<object?>(),
                    It.IsAny<IAuthorizationRequirement[]>())) // استخدام المصفوفة [] بدلاً من الـ Extension Method
                .ReturnsAsync(AuthorizationResult.Failed());

            // --- Act (التنفيذ) ---
            var result = await _handler.Handle(command, CancellationToken.None);

            // --- Assert (التحقق من النتيجة) ---
            result.IsFailure.Should().BeTrue();
        }
    }
}
public class AlwaysValidTotpService : ITotpService
{
    public string GenerateSecretKey()
    {
        throw new NotImplementedException();
    }

    public string GetQrCodeUri(string email, string secretKey, string issuer = "IdentityPlatform")
    {
        throw new NotImplementedException();
    }

    public bool VerifyCode(string secret, string code) => true;
    // أضف أي دوال أخرى تتطلبها الواجهة ITotpService وتُرجع true أو Task.FromResult(true)
}