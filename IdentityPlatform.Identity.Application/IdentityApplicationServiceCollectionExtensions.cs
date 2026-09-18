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

            // تسجيل جميع الـ Validators الموجودة في الـ Assembly تلقائياً
            services.AddValidatorsFromAssembly(assembly);


            return services;
        }
    }
}
