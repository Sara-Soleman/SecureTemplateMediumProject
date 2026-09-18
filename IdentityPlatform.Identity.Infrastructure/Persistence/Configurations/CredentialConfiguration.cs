using Common.Domain;
using IdentityPlatform.Identity.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace IdentityPlatform.Identity.Infrastructure.Persistence.Configurations
{
    public class CredentialConfiguration : IEntityTypeConfiguration<Credential>
    {
        public void Configure(EntityTypeBuilder<Credential> builder)
        {
            builder.HasKey(c => c.Id);

            builder.Property(c => c.Id)
            .HasConversion(
                id => id.Value, // استبدل .Value بالخاصية البدائية التي تخزن الـ Guid داخل Id<T>
                value => new Id<Credential>(value) // أو الطريقة المستخدمة عندك لإنشاء الـ Id مثل new Id<Credential>(value)
            );

            builder.Property(c => c.UserId)
                .IsRequired();

            builder.Property(c => c.PasswordHash)
                .IsRequired();

            builder.Property(c => c.PasswordChangedAt)
                .IsRequired();

            builder.Property(c => c.PasswordVersion)
                .IsRequired();
        }
    }
}
