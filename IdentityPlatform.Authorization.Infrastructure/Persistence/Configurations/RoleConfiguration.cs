using IdentityPlatform.Authorization.Domain.Roles;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Text.Json;

namespace IdentityPlatform.Authorization.Infrastructure.Persistence.Configurations
{
    public sealed class RoleConfiguration : IEntityTypeConfiguration<Role>
    {
        public void Configure(EntityTypeBuilder<Role> builder)
        {
            var comparer = new Microsoft.EntityFrameworkCore.ChangeTracking.ValueComparer<IReadOnlyCollection<string>>(
    (c1, c2) => c1!.SequenceEqual(c2!),
    c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
    c => c.ToList()
);

            builder.Property(r => r.Permissions)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new List<string>()
                )
                .Metadata.SetValueComparer(comparer);

            builder.ToTable("Roles");

            builder.HasKey(r => r.Id);

            
            builder.Property(r => r.Id)
                .HasConversion(id => id.Value, value => new Common.Domain.Id<Role>(value))
                .ValueGeneratedNever();

            builder.Property(r => r.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(r => r.Description)
                .HasMaxLength(250);

            // تخزين قائمة الصلاحيات كـ JSON في قاعدة البيانات لسهولة التعامل معها
            builder.Property(r => r.Permissions)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new List<string>()
                )
                .HasColumnName("PermissionsJson");
        }
    }

    
}
