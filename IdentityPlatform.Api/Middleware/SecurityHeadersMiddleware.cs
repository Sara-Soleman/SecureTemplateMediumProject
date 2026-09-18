namespace IdentityPlatform.Api.Middleware
{
    public static class SecurityHeadersMiddleware
    {
        public static IApplicationBuilder UseSecurityHeaders(this IApplicationBuilder app)
        {
            return app.Use(async (context, next) =>
            {
                var headers = context.Response.Headers;

                // منع تضمين الموقع داخل IFrame لمنع الـ Clickjacking
                headers["X-Frame-Options"] = "DENY";

                // منع المتصفح من تخمين نوع الملف (MIME-sniffing)
                headers["X-Content-Type-Options"] = "nosniff";

                // تفعيل حماية XSS المدمجة في المتصفحات القديمة
                headers["X-XSS-Protection"] = "1; mode=block";

                // سياسة أمان المحتوى (Content Security Policy - CSP) - مخصصة لتكون صارمة للـ APIs
                headers["Content-Security-Policy"] = "default-src 'none'; frame-ancestors 'none';";

                // فرض استخدام HTTPS دائماً (HSTS) لمدة سنة مع تضمين النطاقات الفرعية
                headers["Strict-Transport-Security"] = "max-age=31536000; includeSubDomains; preload";

                // التحكم في معلومات الـ Referrer
                headers["Referrer-Policy"] = "strict-origin-when-cross-origin";

                // إزالة رأس خادم الـ Kestrel / ASP.NET Core لمنع الكشف عن البصمة التقنية (Fingerprinting)
                headers.Remove("X-Powered-By");
                headers.Remove("Server");

                await next();
            });
        }
    }
}
