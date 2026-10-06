using Common.Application.Abstractions;
using Common.Infrastructure.Abstractions;
using Common.Infrastructure.Caching;
using IdentityPlatform.Authorization.Application.Persistence;
using IdentityPlatform.Authorization.Application.Resource_Based_Authorization;
using IdentityPlatform.Authorization.Domain.Roles.Interfaces;
using IdentityPlatform.Authorization.Infrastructure.Persistence;
using IdentityPlatform.Authorization.Infrastructure.Repositories;
using IdentityPlatform.Authorization.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace IdentityPlatform.Authorization.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddAuthorizationInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {

            //services.AddDbContext<AuthorizationDbContext>(options =>
            //        options.UseSqlServer(configuration.GetConnectionString("IdentityDatabase")));
            services.AddDbContext<AuthorizationDbContext>(options =>
    options.UseSqlServer(configuration.GetConnectionString("IdentityDatabase"))
           .LogTo(Console.WriteLine, Microsoft.Extensions.Logging.LogLevel.Information));

            //services.AddScoped<IUnitOfWork>(provider =>
            //    provider.GetRequiredService<AuthorizationDbContext>());
            services.AddScoped< IAuthorizationUnitOfWork, AuthorizationUnitOfWork>();

            // داخل ملف تكوين الخدمات (مثل DependencyInjection.cs الخاص بطبقة البنية التحتية)
            services.AddScoped<IRoleRepository, RoleRepository>();
            services.AddScoped<RoleRepository>();
            services.AddScoped<IRoleRepository>(provider =>
                new CachedRoleRepository(
                    provider.GetRequiredService<RoleRepository>(),
                    provider.GetRequiredService<CachedRepositoryService>()
                ));
            services.AddScoped<IAuthorizationHandler, RoleOwnerOrAdminAuthorizationHandler>();


            return services;
        }
    }
}
