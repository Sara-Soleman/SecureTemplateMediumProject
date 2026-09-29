using Common.Domain.Errors;
using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace IdentityPlatform.Api.Middleware
{
    public static class ResultExtensions
    {
        public static IActionResult ToLocalizedErrorResult<T>(
    this Result<T, IDomainError> result,
    IStringLocalizer localizer)
        {
            if (result.IsSuccess)
                return new OkObjectResult(result.Value);

            var error = result.Error;

            // 🛑 ترجمة كل مفتاح خطأ تفصيلي باستخدام الـ Localizer
            var translatedErrors = error.Errors?
                .Select(errKey => localizer[errKey].Value ?? errKey) // سيقوم بالبحث عن "PasswordMissingNumber" في ملفات الـ .resx العربية
                .ToList() ?? new List<string>();

            var errorResponse = new
            {
                message = localizer[error.ErrorMessage].Value ?? error.ErrorMessage,
                type = error.ErrorType.ToString(),
                errors = translatedErrors
            };
            //string localizedMessage = !string.IsNullOrEmpty(error.ErrorMessage)
            //    ? localizer[error.ErrorMessage]
            //    : "An unexpected error occurred.";

            //var errorResponse = new
            //{
            //    Message = localizedMessage,
            //    Type = error.ErrorType.Name,
            //    Errors = error.Errors
            //};

            //if (error.ErrorType == ErrorType.NotFound || error.ErrorType == ErrorType.Session)
            //{
            //    return new NotFoundObjectResult(errorResponse);
            //}

            return new BadRequestObjectResult(errorResponse);
        }

        
    }
}
