using Common.Application.Abstractions;
using Common.Domain;
using Common.Infrastructure.Abstractions;
using IdentityPlatform.Identity.Domain.Sessions.Interfaces;
using IdentityPlatform.Identity.Domain.Tokens.Interfaces;
using IdentityPlatform.Identity.Domain.Users.Interfaces;
using IdentityPlatform.Identity.Infrastructure.Persistence;
using IdentityPlatform.Identity.Infrastructure.Repositories;
using IdentityPlatform.Identity.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;


namespace IdentityPlatform.Identity.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddIdentityInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            // جلب نص الاتصال من appsettings.json
            var connectionString = configuration.GetConnectionString("IdentityDatabase");
            Console.WriteLine(
    $"Identity connection string exists: {!string.IsNullOrWhiteSpace(connectionString)}");
            // 1. تسجيل IdentityDbContext وإرباحه بـ IUnitOfWork
            services.AddDbContext<IdentityDbContext>(options =>
                options.UseSqlServer(connectionString)); //

            // Register IdentityUnitOfWork as implementation of the context-specific interface
            services.AddScoped<IdentityPlatform.Identity.Application.Persistence.IIdentityUnitOfWork, IdentityUnitOfWork>();

            // 2. تسجيل واجهات الـ Domain (Repositories)
            services.AddScoped<IUserRepository, UserRepository>();

            // 3. تسجيل خدمات الهوية (PasswordHasher & JwtTokenGenerator)
            // (تأكد من إنشاء كلاسات التطبيق الفعلي لها في طبقة Infrastructure وربطها هنا)
            
            services.AddScoped<IPasswordHasher, PasswordHasher>();
            services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
            services.AddScoped<ITotpService, TotpService>();
            services.AddScoped<IUserSessionRepository, UserSessionRepository>();
            services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
            services.AddScoped<ICurrentUserService, CurrentUserService>();

            services.AddScoped<IAuditLogRepository, AuditLogRepository>();

            services.AddHttpContextAccessor();
            return services;

        }
    }
}
