using Common.Application.Abstractions;
using Common.Application.Abstractions.DomainEvents;
using Common.Domain;
using IdentityPlatform.Authorization.Domain.Roles;
using IdentityPlatform.Authorization.Infrastructure.Persistence.Configurations;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace IdentityPlatform.Authorization.Infrastructure.Persistence
{
    public sealed class AuthorizationDbContext : DbContext, IUnitOfWork
    {
        private readonly IPublisher _publisher;
        private readonly IDomainEventDispatcher _dispatcher;
        public AuthorizationDbContext(DbContextOptions<AuthorizationDbContext> options,
            IPublisher publisher,
            IDomainEventDispatcher dispatcher) : base(options)
        {
            _publisher = publisher ?? throw new ArgumentNullException(nameof(publisher));
            _dispatcher = dispatcher;
        }


        //ROLES
        public DbSet<Role> Roles => Set<Role>();
        public DbSet<UserRole> UserRoles => Set<UserRole>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            var typesToIgnore = modelBuilder.Model.GetEntityTypes()
       .Where(t => t.ClrType.IsGenericType && t.ClrType.GetGenericTypeDefinition() == typeof(Id<>))
       .Select(t => t.ClrType)
       .ToList();

            
            

            modelBuilder.ApplyConfiguration(new RoleConfiguration());
            modelBuilder.ApplyConfiguration(new UserRoleConfiguration());


            foreach (var type in typesToIgnore)
            {
                modelBuilder.Ignore(type);
            }

            // تطبيق جميع الـ Configurations الموجودة في نفس الـ Assembly تلقائياً
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AuthorizationDbContext).Assembly);

        }
    }
}
