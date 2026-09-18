using Common.Domain;
using IdentityPlatform.Identity.Domain.Tokens;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace IdentityPlatform.Identity.Infrastructure.Persistence.Configurations
{
    public sealed class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
    {
        public void Configure(EntityTypeBuilder<RefreshToken> builder)
        {
            builder.ToTable("RefreshTokens");

            // 1. المفتاح الأساسي للـ RefreshToken
            builder.HasKey(e => e.Id);
            builder.Property(e => e.Id)
                .HasConversion(
                    id => id.Value,
                    value => new Id<RefreshToken>(value))
                .IsRequired();

            // 2. مفتاح العائلة (FamilyId)
            builder.Property(e => e.FamilyId)
                .HasConversion(
                    id => id.Value,
                    value => new Id<RefreshTokenFamily>(value))
                .IsRequired();

            builder.Property(e => e.TokenHash)
                .HasMaxLength(500)
                .IsRequired();

            builder.Property(e => e.ExpiresAt)
                .IsRequired();

            // العلاقة
            builder.HasOne(e => e.Family)
                .WithMany(f => f.RefreshTokens)
                .HasForeignKey(e => e.FamilyId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
