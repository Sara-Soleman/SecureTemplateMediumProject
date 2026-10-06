using Common.Application.Abstractions;
using Common.Application.Abstractions.CQRS;
using Common.Application.Abstractions.DomainEvents;
using Common.Application.Abstractions.Handlers;
using Common.Application.Events.Dispatchers;
using Common.Domain;
using Common.Domain.Errors;
using CSharpFunctionalExtensions;
using IdentityPlatform.Authorization.Application.Persistence;
using IdentityPlatform.Authorization.Domain.Roles;
using IdentityPlatform.Authorization.Domain.Roles.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace IdentityPlatform.Authorization.Application.Roles.Commands.CreateRole
{
    public class CreateRoleCommandHandler : CommandHandlerBase<CreateRoleCommand,Guid, IAuthorizationUnitOfWork>
    {
        private readonly IRoleRepository _roleRepository;
        private Role _role;

        public CreateRoleCommandHandler(IRoleRepository roleRepository, IAuthorizationUnitOfWork unitOfWork,
            IDomainEventDispatcher domainEventDispatcher)
            : base(domainEventDispatcher, unitOfWork)

        {
            _roleRepository = roleRepository;
        }

        

        protected async override Task<Result<Guid, IDomainError>> ExecuteAsync(CreateRoleCommand request, CancellationToken cancellationToken)
        {
            // 1. التحقق مما إذا كان الدور موجوداً مسبقاً بنفس الاسم
            var existingRole = await _roleRepository.GetByNameAsync(request.Name, cancellationToken);
            if (existingRole != null)
            {
                return Result.Failure<Guid, IDomainError>(DomainError.RoleAlreadyExists(request.Name));
                // (تأكدي من إضافة خطأ مناسب في DomainErrors لديك أو استخدام الـ Error القياسي)
            }

            // 2. إنشاء كيان الدور الجديد باستخدام الـ Domain Factory
            var role = Role.Create(request.Name, request.Description, request.Permissions);

            // 3. إضافته للمستودع
            await _roleRepository.AddAsync(role, cancellationToken);

            // إرجاع معرّف الدور الجديد بنجاح
            return Result.Success<Guid, IDomainError>(role.Id.Value);
        }

        protected override IAggregateRoot? GetAggregateRoot(Result<Guid, IDomainError> result)
        {
            return _role;
        }
    }
}
