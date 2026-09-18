using Common.Application;
using Common.Domain;
using IdentityPlatform.Api.Middleware;
using IdentityPlatform.Identity.Application;
using IdentityPlatform.Identity.Domain.Sessions;
using IdentityPlatform.Identity.Infrastructure;
using IdentityPlatform.Identity.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// 1. إضافة خدمات YARP وقراءتها من appsettings.json
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

builder.Services.AddApplicationLayer();
// في ملف Program.cs
builder.Services.AddIdentityApplication();



builder.Services.AddIdentityInfrastructure(builder.Configuration);

#region JWT
var jwtSettings = builder.Configuration.GetSection("Jwt");
string secretKey = jwtSettings["Secret"] ?? "YourSuperSecretKeyHereThatIsLongEnough12345!";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = false,   // معطلة مؤقتاً لتجاوز أي عدم مطابقة
        ValidateAudience = false, // معطلة مؤقتاً لتجاوز أي عدم مطابقة
        ValidateLifetime = true,  // التحقق من صلاحية وقت التوكن
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
        ValidateIssuerSigningKey = true
    };

    // تتبع أسباب الرفض بدقة في حال حدوث أي خطأ لاحقاً
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
        ValidateIssuerSigningKey = true
    };

    options.Events = new JwtBearerEvents
    {
        OnTokenValidated = async context =>
        {
            var dbContext = context.HttpContext.RequestServices.GetRequiredService<IdentityDbContext>();

            var sidClaim = context.Principal?.FindFirst("sid")?.Value;

            if (string.IsNullOrEmpty(sidClaim) || !Guid.TryParse(sidClaim, out var sessionIdGuid))
            {
                context.Fail("Invalid session identifier.");
                return;
            }

            var targetSessionId = new Id<UserSession>(sessionIdGuid);
            var now = DateTimeOffset.UtcNow;

            // التعديل الجوهري هنا: التحقق أن الجلسة موجودة، غير ملغاة، ولم تنته صلاحيتها
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
builder.Services.AddSwaggerGen();
#endregion
builder.Services.AddHttpContextAccessor();
builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

#region YARP
// 2. تطبيق الحماية والأمان (Security Headers والـ Rate Limiting العامة إن وجدت)
app.Use(async (context, next) =>
{
    context.Response.Headers["X-Content-Type-Options"] = "nosniff";
    context.Response.Headers["X-Frame-Options"] = "DENY";
    context.Response.Headers.Remove("Server");
    await next();

});


// قراءة الـ IP الحقيقي المرسل من الـ Reverse Proxy عبر X-Forwarded-For / X-Forwarded-Proto
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

#region Response Header Middleware
//لحماية تطبيق الـ API من هجمات مثل XSS, Clickjacking, و MIME-sniffing
app.UseSecurityHeaders();
#endregion
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapReverseProxy();
app.MapControllers();

app.Run();
