using Common.Application.Abstractions;
using Common.Application.Abstractions.DomainEvents;
using Common.Domain;
using IdentityPlatform.Identity.Domain.Sessions;
using IdentityPlatform.Identity.Domain.Tokens;
using IdentityPlatform.Identity.Domain.Users;
using IdentityPlatform.Identity.Infrastructure.Persistence.Configurations;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Net;
using System.Reflection.Emit;
using System.Text;


namespace IdentityPlatform.Identity.Infrastructure.Persistence
{
    public class IdentityDbContext : DbContext, IUnitOfWork
    {
        private readonly IPublisher _publisher;
        private readonly IDomainEventDispatcher _dispatcher;
        public IdentityDbContext(DbContextOptions<IdentityDbContext> options,
            IPublisher publisher,
            IDomainEventDispatcher dispatcher) : base(options)
        {
            _publisher = publisher ?? throw new ArgumentNullException(nameof(publisher));
            _dispatcher = dispatcher;
        }

        public DbSet<User> Users => Set<User>();
        public DbSet<Credential> Credentials => Set<Credential>();
        public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
        public DbSet<RefreshTokenFamily> RefreshTokenFamilies => Set<RefreshTokenFamily>();
        public DbSet<UserSession> UserSessions => Set<UserSession>();






        public DbSet<AuditLog> AuditLogs { get; set; } = null!;
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);


            modelBuilder.Entity<AuditLog>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.ActionName).IsRequired().HasMaxLength(150);
                entity.Property(e => e.UserId);
                entity.Property(e => e.Timestamp).IsRequired();
            });



            var typesToIgnore = modelBuilder.Model.GetEntityTypes()
        .Where(t => t.ClrType.IsGenericType && t.ClrType.GetGenericTypeDefinition() == typeof(Id<>))
        .Select(t => t.ClrType)
        .ToList();
            modelBuilder.ApplyConfiguration(new UserConfiguration());
            modelBuilder.ApplyConfiguration(new UserSessionConfiguration());
            modelBuilder.ApplyConfiguration(new CredentialConfiguration());
            modelBuilder.ApplyConfiguration(new RefreshTokenConfiguration());
            modelBuilder.ApplyConfiguration(new RefreshTokenFamilyConfiguration());

           

            foreach (var type in typesToIgnore)
            {
                modelBuilder.Ignore(type);
            }

            // تطبيق جميع الـ Configurations الموجودة في نفس الـ Assembly تلقائياً
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(IdentityDbContext).Assembly);


        }
        //public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        //{
        //    // أ. استخراج كل الـ Aggregates التي تحتوي على Domain Events معلقة قبل الحفظ
        //    var domainEntities = ChangeTracker
        //        .Entries<IAggregateRoot>()
        //        .Where(x => x.Entity.DomainEvents.Any())
        //        .Select(x => x.Entity)
        //        .ToList();

        //    var domainEvents = domainEntities
        //        .SelectMany(x => x.PopDomainEvents())
        //        .ToList();

        //    // ب. الحفظ الفعلي في قاعدة البيانات (Transactional Boundary)
        //    var result = await base.SaveChangesAsync(cancellationToken);

        //    // ج. نشر الأحداث عبر MediatR بعد نجاح الحفظ لتلتقطها الـ Handlers (مثل Serilog)
        //    foreach (var domainEvent in domainEvents)
        //    {
        //        await _publisher.Publish(domainEvent, cancellationToken);
        //    }
        //    // نشر الأحداث عبر الـ Dispatcher 
        //    if (domainEvents.Any())
        //    {
        //        await _dispatcher.DispatchAsync(domainEvents, cancellationToken);
        //    }

        //    return result;
        //}
    }
}
