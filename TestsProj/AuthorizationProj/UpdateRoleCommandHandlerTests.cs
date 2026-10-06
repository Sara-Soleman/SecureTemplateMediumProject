using FluentAssertions;
using IdentityPlatform.Authorization.Application.Resource_Based_Authorization;
using IdentityPlatform.Authorization.Application.Roles.Commands.UpdateRoles;
using IdentityPlatform.Authorization.Application.Roles.Dtos;
using IdentityPlatform.Authorization.Domain.Roles;
using IdentityPlatform.Authorization.Domain.Roles.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Moq;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;

namespace TestsProj.AuthorizationProj
{
    public class UpdateRoleCommandHandlerTests
    {
        private readonly Mock<IAuthorizationService> _authorizationServiceMock;
        private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
        private readonly Mock<IRoleRepository> _roleRepositoryMock;
        private readonly UpdateRoleCommandHandler _handler;

        public UpdateRoleCommandHandlerTests()
        {
            _authorizationServiceMock = new Mock<IAuthorizationService>();
            _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            _roleRepositoryMock = new Mock<IRoleRepository>();

            _handler = new UpdateRoleCommandHandler(
                _authorizationServiceMock.Object,
                _httpContextAccessorMock.Object,
                _roleRepositoryMock.Object
            );
        }

        [Fact]
        public async Task Handle_WhenUserTriesToModifySystemRole_ShouldReturnUnauthorizedResult()
        {
            // Arrange (التحضير)
            var roleId = Guid.NewGuid();
            var command = new UpdateRoleCommand(roleId, "New Name", "New Description", new List<string> { "Perm.1" });

            // دور من نوع System Role (لا يمكن تعديله)
            var permissionsList = new List<string> { "Permissions.Roles.View" };

            var role = Role.Create("Admin", "System Admin Role", permissionsList);

            _roleRepositoryMock.Setup(repo => repo.GetByIdAsync(roleId, It.IsAny<CancellationToken>()))
                               .ReturnsAsync(role);

            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "user-123") }, "TestAuth"));

            _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(new DefaultHttpContext { User = claimsPrincipal });

            // محاكاة فشل خدمة التفويض (Resource-Based Authorization رفضت الطلب)
           
            _authorizationServiceMock.Setup(x => x.AuthorizeAsync(
                    It.IsAny<ClaimsPrincipal>(),
                    It.IsAny<object?>(),
                    It.IsAny<IEnumerable<IAuthorizationRequirement>>()))
                .ReturnsAsync(AuthorizationResult.Failed());

            // Act (التنفيذ)
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert (التحقق من النتيجة)
            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be("Unauthorized");
        }
    }
}
