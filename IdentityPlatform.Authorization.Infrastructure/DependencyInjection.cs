using Common.Infrastructure.Caching;
using IdentityPlatform.Authorization.Domain.Roles.Interfaces;
using IdentityPlatform.Authorization.Infrastructure.Repositories;
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
        public static IServiceCollection AddAuthorizationInfrastructure(this IServiceCollection services)
        {


            // داخل ملف تكوين الخدمات (مثل DependencyInjection.cs الخاص بطبقة البنية التحتية)
            services.AddScoped<IRoleRepository, RoleRepository>();
            services.AddScoped<RoleRepository>();
            services.AddScoped<IRoleRepository>(provider =>
                new CachedRoleRepository(
                    provider.GetRequiredService<RoleRepository>(),
                    provider.GetRequiredService<CachedRepositoryService>()
                ));
            return services;
        }
    }
}
