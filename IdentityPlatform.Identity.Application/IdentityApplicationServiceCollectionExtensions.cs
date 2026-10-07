using Common.Application.Behaviours;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace IdentityPlatform.Identity.Application
{
    public static class IdentityApplicationServiceCollectionExtensions
    {
        public static IServiceCollection AddIdentityApplication(this IServiceCollection services)
        {

            var assembly = Assembly.GetExecutingAssembly();

            // تسجيل الـ MediatR مع إضافة الـ Validation Pipeline Behavior
            services.AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssembly(assembly);
                cfg.AddOpenBehavior(typeof(LoggingPipelineBehaviour<,>));
                cfg.AddOpenBehavior(typeof(ValidationPipelineBehaviour<,>));
            });
            var handlerServices = services
    .Where(x =>
        x.ServiceType.FullName?.Contains("IRequestHandler") == true &&
        x.ServiceType.FullName?.Contains("RegisterUser") == true)
    .ToList();

            foreach (var service in handlerServices)
            {
                Console.WriteLine(
                    $"MEDIATR SERVICE: {service.ServiceType.FullName} -> {service.ImplementationType?.FullName}");
            }

            // تسجيل جميع الـ Validators الموجودة في الـ Assembly تلقائياً
            services.AddValidatorsFromAssembly(assembly);


            return services;
        }
    }
}
