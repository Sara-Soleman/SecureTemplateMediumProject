using Common.Application;
using Common.Application.Abstractions.DomainEvents;
using Common.Application.Behaviours;
using Common.Domain;
using Common.Domain.Errors;
using Common.Infrastructure.Caching;
using CSharpFunctionalExtensions;
using IdentityPlatform.Api.Middleware;
using IdentityPlatform.Authorization.Application;
using IdentityPlatform.Authorization.Domain;
using IdentityPlatform.Authorization.Infrastructure;
using IdentityPlatform.Authorization.Infrastructure.Services;
using IdentityPlatform.Identity.Application;
using IdentityPlatform.Identity.Application.Authorization;
using IdentityPlatform.Identity.Application.Persistence;
using IdentityPlatform.Identity.Application.Users.Commands.RegisterUser;
using IdentityPlatform.Identity.Domain.Sessions;
using IdentityPlatform.Identity.Domain.Users;
using IdentityPlatform.Identity.Domain.Users.Interfaces;
using IdentityPlatform.Identity.Infrastructure;
using IdentityPlatform.Identity.Infrastructure.Persistence;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Microsoft.OpenApi;
using Serilog;
using Serilog.Formatting.Compact;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.RateLimiting;


#region seriLog 
var builder = WebApplication.CreateBuilder(args);
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .Enrich.FromLogContext()
    .Enrich.WithMachineName()
    .Enrich.WithEnvironmentName()
    .Enrich.WithEnvironmentUserName()
    
    // كتابة السجلات بصيغة Compact JSON في مجلد Logs يومياً
    .WriteTo.File(
        new CompactJsonFormatter(),
        "Logs/security-audit-.json",
        rollingInterval: RollingInterval.Day)
    .WriteTo.Seq("http://localhost:5341")
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

try
{
    Log.Information("Starting web application...");
   

    // ربط Serilog مع الـ Host الخاص بـ ASP.NET Core
    builder.Host.UseSerilog();

#endregion

    #region Cache 
    builder.Services.AddMemoryCache();
    builder.Services.AddSingleton<CachedRepositoryService>();
    #endregion

    //var builder = WebApplication.CreateBuilder(args);

    // Add services to the container.
    // 1. إضافة خدمات YARP وقراءتها من appsettings.json
    builder.Services.AddReverseProxy()
        .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

    builder.Services.AddApplicationLayer();
    // في ملف Program.cs
    builder.Services.AddIdentityApplication();

    // في ملف Program.cs أو في Extension Method الخاصة بالتسجيل
    builder.Services.AddScoped<IAuthorizationReader, AuthorizationReader>();


    builder.Services.AddAuthorizationInfrastructure(builder.Configuration);

    builder.Services.AddIdentityInfrastructure(builder.Configuration);
    builder.Services.AddAuthorizationApplication();
    #region JWT
    var jwtSettings = builder.Configuration.GetSection("Jwt");
    string secretKey = jwtSettings["Secret"] ?? "YourSuperSecretKeyHereThatIsLongEnough12345!";
    string issuer = jwtSettings["Issuer"] ;
    string audience = jwtSettings["Audience"] ;

    builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        

        // تتبع أسباب الرفض بدقة في حال حدوث أي خطأ لاحقاً
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = issuer,
            ValidateAudience = true,
            ValidAudience = audience,
            ValidateLifetime = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
            ValidateIssuerSigningKey = true
        };

        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = async context =>
            {
                var dbContext = context.HttpContext.RequestServices.GetRequiredService<IdentityDbContext>();

                // 1. استخراج الـ Claims
                var allClaims = context.Principal?.Claims.Select(c => $"{c.Type}: {c.Value}").ToList();
                var userIdClaim = context.Principal?.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
               ?? context.Principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value; 
                
                var tokenVersionClaim = context.Principal?.FindFirst("tokenVersion")?.Value;
                var sidClaim = context.Principal?.FindFirst("sid")?.Value;

                if (string.IsNullOrEmpty(userIdClaim) || string.IsNullOrEmpty(tokenVersionClaim) || string.IsNullOrEmpty(sidClaim))
                {
                    context.Fail("Invalid token claims.");
                    return;
                }

                //var userId = Guid.Parse(userIdClaim);
                var tokenVersionFromToken = int.Parse(tokenVersionClaim);
                var sessionIdGuid = Guid.Parse(sidClaim);
                var targetSessionId = new Id<UserSession>(sessionIdGuid);
                var now = DateTimeOffset.UtcNow;

                var userIdGuid = Guid.Parse(userIdClaim);
                var targetUserId = new Id<User>(userIdGuid);

                // 2. التحقق من أن إصدار التوكن يطابق الإصدار الحالي للمستخدم في قاعدة البيانات (للتعامل مع تغيير كلمة المرور)
                // (ملاحظة: تأكدي من نوع الـ Id الخاص بالـ User، هل هو Guid خام أم Value Object مثل Id<User>)
                var user = await dbContext.Users
                        .FirstOrDefaultAsync(u => u.Id == targetUserId);
                if (user == null || user.TokenVersion != tokenVersionFromToken)
                {
                    context.Fail("Token is expired due to password change.");
                    return;
                }

                // 3. التحقق أن الجلسة موجودة، غير ملغاة، ولم تنته صلاحيتها
                var sessionIsValid = await dbContext.UserSessions
                    .AnyAsync(s => s.Id == targetSessionId && s.RevokedAt == null && s.ExpiresAt > now);

                if (!sessionIsValid)
                {
                    context.Fail("This session has been revoked or expired.");
                }
            }
        };
    });
    #endregion

    #region Authorization
    //تم الاستغناء عنها عبر منطق DynamicAuthorizationPolicyProvider//
    //-_- :) ;)
    //builder.Services.AddAuthorization(options =>
    //{

    //    foreach (var permission in Permissions.GetAllPermissions())
    //    {
    //        options.AddPolicy(permission, policy =>
    //            policy.RequireClaim("permission", permission));
    //    }
    //});
    #endregion

    #region Localization
    // 1. إضافة خدمة الترجمة وتحديد مجلد الملفات (Resources)
    builder.Services.AddLocalization(options => options.ResourcesPath = "Resourses");

    // 2. تفعيل الـ Controllers مع دعم الـ DataAnnotations للتوطين إن أردت
    builder.Services.AddControllers()
        .AddDataAnnotationsLocalization();
    #endregion
   

    #region Rate Limiting متعدد المستويات

    // --- 1. إعداد سياسات الـ Rate Limiting ---
    builder.Services.AddRateLimiter(options =>
    {
        // أ) سياسة عامة تعتمد على الـ IP (مثلاً 60 طلب في الدقيقة)
        options.AddPolicy("FixedGlobal", httpContext =>
            RateLimitPartition.GetFixedWindowLimiter(
                partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                factory: _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 60,
                    Window = TimeSpan.FromMinutes(1),
                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                    QueueLimit = 2
                }));

        // ب) سياسة مشددة جداً لنقاط النهاية الحساسة (مثل تغيير كلمة المرور أو تسجيل الدخول)
        // ندمج فيها الـ IP مع محاولة استخراج هوية الحساب إن وجدت لحماية فائقة
        options.AddPolicy("StrictAuth", httpContext =>
        {
            var clientIp = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

            // محاولة جلب اسم المستخدم أو الهوية من الـ Request Body أو الـ Claims إن كان مسجلاً
            // (لعمليات تغيير كلمة المرور يكون المستخدم مسجلاً، أما لـ Login فنعتمد على الـ IP واسم المستخدم المرسل في الـ Body إن أمكن، أو نكتفي بالـ IP المشدد)
            return RateLimitPartition.GetSlidingWindowLimiter(
                partitionKey: clientIp,
                factory: _ => new SlidingWindowRateLimiterOptions
                {
                    PermitLimit = 5,             // 5 محاولات فقط كحد أقصى
                    Window = TimeSpan.FromMinutes(5), // خلال 5 دقائق
                    SegmentsPerWindow = 5,
                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                    QueueLimit = 0               // رفض فوري بدون طابور انتظار
                });
        });

        // تخصيص الرد عند رفض الطلب (HTTP 429 Too Many Requests)
        options.OnRejected = async (context, token) =>
        {
            context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
            context.HttpContext.Response.ContentType = "application/json";
            await context.HttpContext.Response.WriteAsync(
                "{\"error\": \"تم تجاوز الحد الأقصى للطلبات المسموحة. يرجى المحاولة لاحقاً.\"}",
                token);
        };
    });
    #endregion



    #region Swagger
    builder.Services.AddSwaggerGen(options =>
    {
        options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Description = "Enter your JWT Bearer token.",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT"
        });

        options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
        {
            [new OpenApiSecuritySchemeReference("Bearer", document)] = []
        });
    });
    #endregion
    builder.Services.AddHttpContextAccessor();
    builder.Services.AddControllers();
    // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
    builder.Services.AddOpenApi();

    var app = builder.Build();



    using (var scope = app.Services.CreateScope())
    {
        Console.WriteLine("1 - Before DbContextOptions");

        var options = scope.ServiceProvider
            .GetRequiredService<DbContextOptions<IdentityDbContext>>();

        Console.WriteLine("2 - DbContextOptions OK");

        Console.WriteLine("3 - Before IdentityDbContext");

        try
        {
            var context = scope.ServiceProvider
                .GetRequiredService<IdentityDbContext>();

            Console.WriteLine("4 - IdentityDbContext OK");
        }
        catch (Exception ex)
        {
            Console.WriteLine("!!! EXCEPTION !!!");
            Console.WriteLine(ex.ToString());
        }
    }




    #region YARP & Proxy Headers
    // 1. قراءة الـ IP الحقيقي أولاً وقبل كل شيء (ممتاز جداً وضعه في البداية)
    app.UseForwardedHeaders(new ForwardedHeadersOptions
    {
        ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
    });

    
    #endregion

    #region Swagger
    if (app.Environment.IsDevelopment())
    {
        // تفعيل الـ Swagger UI
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "Identity Platform API V1");
            c.RoutePrefix = string.Empty; // لجعل الـ Swagger يفتح مباشرة على الصفحة الرئيسية للنظام (اختياري)
        });
    }
    #endregion
    // Configure the HTTP request pipeline.
    //if (app.Environment.IsDevelopment())
    //{
    //    app.MapOpenApi();
    //}

    #region Localization
    var supportedCultures = new[] { "en", "ar" };
    var localizationOptions = new RequestLocalizationOptions()
        .SetDefaultCulture("en")
        .AddSupportedCultures(supportedCultures)
        .AddSupportedUICultures(supportedCultures);

    app.UseRequestLocalization(localizationOptions);
    #endregion

    #region Response Header Middleware
    //لحماية تطبيق الـ API من هجمات مثل XSS, Clickjacking, و MIME-sniffing
    app.UseSecurityHeaders();
    #endregion

    app.UseHttpsRedirection();
    app.UseRateLimiter();

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapReverseProxy();
    app.MapControllers();
    app.UseMiddleware<GlobalExceptionHandlerMiddleware>();
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly.");
}
finally
{
    Log.CloseAndFlush();
}