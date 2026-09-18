using Common.Domain;
using IdentityPlatform.Identity.Domain.Sessions;
using IdentityPlatform.Identity.Domain.Tokens;
using IdentityPlatform.Identity.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace IdentityPlatform.Identity.Infrastructure.Persistence.Configurations
{
    public class UserSessionConfiguration : IEntityTypeConfiguration<UserSession>
    {
        public void Configure(EntityTypeBuilder<UserSession> builder)
        {
            builder.HasKey(u => u.Id);

            builder.Property(e => e.Id)
                 .HasConversion(
                     id => id.Value,
                     value => new Id<UserSession>(value))
                 .IsRequired();

            builder.Property(s => s.UserId)
                .IsRequired();

            builder.Property(s => s.RefreshToken)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(s => s.IpAddress)
                .HasMaxLength(45); // يدعم IPv4 و IPv6

            builder.Property(s => s.UserAgent)
                .HasMaxLength(500);

            builder.Property(s => s.ExpiresAt)
                .IsRequired();

            builder.Property(s => s.CreatedAt)
                .IsRequired();

            builder.Property(s => s.RevokedAt);

            // ربط الجلسة بالمستخدم (اختياري بحسب وجود علاقة التنقل Navigation Property في كلاس User)
            builder.HasIndex(s => s.RefreshToken);
            builder.HasIndex(s => s.UserId);
        }
    }
}
