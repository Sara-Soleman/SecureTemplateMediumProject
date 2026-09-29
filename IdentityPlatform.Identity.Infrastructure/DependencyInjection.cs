using Common.Application.Abstractions;
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
            // 1. تسجيل IdentityDbContext وإرباحه بـ IUnitOfWork
            services.AddDbContext<IdentityDbContext>(options =>
                options.UseSqlServer(connectionString)); // أو UseNpgsql / UseSqlite حسب نوع قاعدة البيانات لديك

            services.AddScoped<IUnitOfWork>(provider =>
                provider.GetRequiredService<IdentityDbContext>());

            // 2. تسجيل واجهات الـ Domain (Repositories)
            services.AddScoped<IUserRepository, UserRepository>();

            // 3. تسجيل خدمات الهوية (PasswordHasher & JwtTokenGenerator)
            // (تأكد من إنشاء كلاسات التطبيق الفعلي لها في طبقة Infrastructure وربطها هنا)
            
            services.AddScoped<IPasswordHasher, PasswordHasher>();
            services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
            services.AddScoped<ITotpService, TotpService>();
            services.AddScoped<IUserSessionRepository, UserSessionRepository>();
            services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
            return services;
        }
    }
}
