using Common.Application.Abstractions;
using Common.Domain;
using IdentityPlatform.Identity.Domain.Sessions;
using IdentityPlatform.Identity.Domain.Tokens;
using IdentityPlatform.Identity.Domain.Users;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Net;
using System.Reflection.Emit;
using System.Text;

namespace IdentityPlatform.Identity.Infrastructure.Persistence
{
    public class IdentityDbContext : DbContext, IUnitOfWork
    {
        public IdentityDbContext(DbContextOptions<IdentityDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users => Set<User>();
        public DbSet<Credential> Credentials => Set<Credential>();
        public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
        public DbSet<RefreshTokenFamily> RefreshTokenFamilies => Set<RefreshTokenFamily>();
        public DbSet<UserSession> UserSessions => Set<UserSession>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            var typesToIgnore = modelBuilder.Model.GetEntityTypes()
        .Where(t => t.ClrType.IsGenericType && t.ClrType.GetGenericTypeDefinition() == typeof(Id<>))
        .Select(t => t.ClrType)
        .ToList();

            foreach (var type in typesToIgnore)
            {
                modelBuilder.Ignore(type);
            }

            // تطبيق جميع الـ Configurations الموجودة في نفس الـ Assembly تلقائياً
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(IdentityDbContext).Assembly);


        }
    }
}
