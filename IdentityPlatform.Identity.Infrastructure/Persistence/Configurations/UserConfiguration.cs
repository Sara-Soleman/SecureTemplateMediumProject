using Common.Domain;
using IdentityPlatform.Identity.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace IdentityPlatform.Identity.Infrastructure.Persistence.Configurations
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.HasKey(u => u.Id);

            builder.Property(u => u.Id)
           .HasConversion(
               id => id.Value, // استبدل .Value بالخاصية البدائية التي تخزن الـ Guid داخل Id<T>
               value => new Id<User>(value) // أو الطريقة المستخدمة عندك لإنشاء الـ Id مثل new Id<Credential>(value)
           );

            builder.Property(u => u.Username)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(u => u.Email)
                .IsRequired()
                .HasMaxLength(256);

            builder.HasIndex(u => u.Email)
                .IsUnique();

            builder.Property(u => u.AccountStatus)
                .IsRequired();

            builder.Property(u => u.TokenVersion)
                .IsRequired();

            builder.Property(u => u.PasswordResetTokenHash)
                .HasMaxLength(500);

            builder.Property(u => u.PasswordResetTokenExpiresAt)
                .IsRequired(false);

            builder.Property(u => u.CreatedAt)
                .IsRequired();

            builder.Property(u => u.UpdatedAt)
                .IsRequired(false);

            // العلاقة 1 إلى 1 مع Credential
            builder.HasOne(u => u.Credential)
                .WithOne(c => c.User)
                .HasForeignKey<Credential>(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
