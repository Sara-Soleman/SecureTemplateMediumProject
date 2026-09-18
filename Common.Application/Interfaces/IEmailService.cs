using System;
using System.Collections.Generic;
using System.Text;

namespace Common.Application.Interfaces
{
    public interface IEmailService
    {
        Task SendPasswordResetEmailAsync(string email, string resetToken, CancellationToken cancellationToken);
        Task SendEmailAsync(string to, string subject, string body, CancellationToken cancellationToken = default);
    }
}
