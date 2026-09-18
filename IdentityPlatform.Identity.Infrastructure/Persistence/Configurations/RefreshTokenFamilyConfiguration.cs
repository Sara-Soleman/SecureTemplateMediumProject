using Common.Domain;
using IdentityPlatform.Identity.Domain.Tokens;
using IdentityPlatform.Identity.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace IdentityPlatform.Identity.Infrastructure.Persistence.Configurations
{
    public sealed class RefreshTokenFamilyConfiguration : IEntityTypeConfiguration<RefreshTokenFamily>
    {
        public void Configure(EntityTypeBuilder<RefreshTokenFamily> builder)
        {
            builder.ToTable("RefreshTokenFamilies");

            // 1. المفتاح الأساسي للعائلة
            builder.HasKey(e => e.Id);
            builder.Property(e => e.Id)
                .HasConversion(
                    id => id.Value,
                    value => new Id<RefreshTokenFamily>(value))
                .IsRequired();

            // 2. معرف المستخدم (UserId)
            builder.Property(e => e.UserId)
                .HasConversion(
                    id => id.Value,
                    value => new Id<User>(value))
                .IsRequired();

            builder.Property(e => e.SessionId)
                .IsRequired();

            builder.Property(e => e.CreatedAt)
                .IsRequired();

            builder.Property(e => e.ExpiresAt)
                .IsRequired();
        }
    }
}
