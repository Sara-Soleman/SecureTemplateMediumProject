using Common.Application.Abstractions.DomainEvents;
using Common.Application.Behaviours;
using Common.Application.Email;
using Common.Application.Events.Dispatchers;
using Common.Application.Interfaces;
using Common.Domain.Events;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Common.Application
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddApplicationLayer(
            this IServiceCollection services,
            params Assembly[] assemblies)
        {

            if (assemblies == null || assemblies.Length == 0)
            {
                // يمكنك هنا جلب الـ Assemblies المسجلة حالياً أو تحديد Assembly افتراضي
                assemblies = AppDomain.CurrentDomain.GetAssemblies()
                    .Where(a => a.FullName?.StartsWith("IdentityPlatform") == true || a.FullName?.StartsWith("Common") == true)
                    .ToArray();
            }

            // 1. تسجيل MediatR والـ Pipeline Behaviors
            services.AddMediatR(cfg =>
            {
                foreach (var assembly in assemblies)
                {
                    cfg.RegisterServicesFromAssembly(assembly);
                }

                // ترتيب الـ Pipeline Behaviors (مهم جداً بالترتيب أدناه)
                cfg.AddOpenBehavior(typeof(LoggingPipelineBehaviour<,>));
                cfg.AddOpenBehavior(typeof(ValidationPipelineBehaviour<,>));
                cfg.AddOpenBehavior(typeof(CommandMetricsBehavior<,>));
                cfg.AddOpenBehavior(typeof(ExceptionHandlingPipelineBehavior<,>));
            });

            // 2. تسجيل الـ DomainEventDispatcher
            services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();
            services.AddScoped<IEmailService, EmailService>();

            // 3. تسجيل الـ Validators والـ Domain Event Handlers لجميع الـ Assemblies الممررة
            foreach (var assembly in assemblies)
            {
                // تسجيل FluentValidation
                services.AddValidatorsFromAssembly(assembly);

                // تسجيل الـ Domain Event Handlers باستخدام Scrutor
                services.Scan(scan => scan
                    .FromAssemblies(assembly)
                    .AddClasses(classes => classes.AssignableTo(typeof(IDomainEventHandler<>)))
                    .AsImplementedInterfaces()
                    .WithScopedLifetime());
            }

            return services;
        }
    }

}
