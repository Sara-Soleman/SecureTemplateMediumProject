using Common.Domain;
using IdentityPlatform.Authorization.Domain.Roles;
using IdentityPlatform.Identity.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace IdentityPlatform.Identity.Infrastructure.Persistence.Configurations
{
    public sealed class UserRoleConfiguration : IEntityTypeConfiguration<UserRole>
    {
        public void Configure(EntityTypeBuilder<UserRole> builder)
        {
            builder.ToTable("UserRoles");

            // مفتاح مركب (Composite Key) يمنع تكرار نفس الدور لنفس المستخدم
            builder.HasKey(ur => new { ur.UserId, ur.RoleId });

            builder.Property(ur => ur.UserId)
                .HasConversion(id => id.Value, value => new Id<User>(value));

            builder.Property(ur => ur.RoleId)
                .HasConversion(id => id.Value, value => new Id<Role>(value));

        }
    }
}
