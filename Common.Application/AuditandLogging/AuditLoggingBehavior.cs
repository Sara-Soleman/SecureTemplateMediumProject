using Common.Domain;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace Common.Application.AuditandLogging
{
    public class AuditLoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
    {
        private readonly ICurrentUserService _currentUserService;
        private readonly IAuditLogRepository _auditLogRepository;

        public AuditLoggingBehavior(ICurrentUserService currentUserService, IAuditLogRepository auditLogRepository)
        {
            _currentUserService = currentUserService;
            _auditLogRepository = auditLogRepository;
        }

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            var requestName = typeof(TRequest).Name;

            // هل هذا الـ Command يحتاج إلى Audit Logging؟ (نستهدف أوامر الأدوار والتعديلات الأمنية)
            bool isAuditable = requestName.Contains("Role") || requestName.Contains("Permission") || requestName.Contains("User")|| requestName.Contains("Sessions");

            var response = await next(); // تنفيذ الطلب الأصلي
            bool isSuccess = true;
            if (response != null)
            {
                // البحث الذكي عن خاصية IsFailure أو IsSuccess باستخدام الـ Reflection أو التحقق المباشر
                var type = response.GetType();

                // إذا كان الكلاس يحتوي على خاصية IsFailure وتساوي true، إذن العملية فشلت!
                var isFailureProperty = type.GetProperty("IsFailure");
                if (isFailureProperty != null)
                {
                    var isFailureValue = (bool)(isFailureProperty.GetValue(response) ?? false);
                    if (isFailureValue)
                    {
                        isSuccess = false;
                    }
                }
                // أو إذا كان يحتوي على IsSuccess وتساوي false
                var isSuccessProperty = type.GetProperty("IsSuccess");
                if (isSuccessProperty != null)
                {
                    var isSuccessValue = (bool)(isSuccessProperty.GetValue(response) ?? true);
                    if (!isSuccessValue)
                    {
                        isSuccess = false;
                    }
                }
            }
                if (isAuditable)
            {
                var auditLog = new AuditLog
                {
                    UserId = _currentUserService.UserId,
                    ActionName = requestName,
                    Parameters = JsonSerializer.Serialize(request),
                    IsSuccess = isSuccess, // يمكن ربطها بنتيجة الـ Result إذا كانت تعيد IsFailure
                    Timestamp = DateTime.UtcNow
                };

                await _auditLogRepository.AddAsync(auditLog, cancellationToken);
            }

            return response;
        }


    }
}
