using Common.Application.Interfaces;

using MailKit.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;


namespace Common.Application.Email
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration config, ILogger<EmailService> logger)
        {
            _config = config;
            _logger = logger;
        }

        // تطبيق دالة الواجهة الموحدة
        public async Task SendPasswordResetEmailAsync(string email, string resetToken, CancellationToken cancellationToken)
        {
            var subject = "استعادة كلمة المرور - IdentityPlatform";

            // تصميم محتوى الإيميل (HTML Body)
            var body = $@"
                <div style='font-family: Arial, sans-serif; direction: rtl; text-align: right;'>
                    <h2>طلب استعادة كلمة المرور</h2>
                    <p>لقد تلقينا طلباً لإعادة تعيين كلمة المرور الخاصة بحسابك.</p>
                    <p>استخدم الرمز التالي لإتمام عملية التغيير:</p>
                    <div style='background: #f4f4f4; padding: 10px; font-size: 20px; font-weight: bold; width: fit-content; letter-spacing: 2px;'>
                        {resetToken}
                    </div>
                    <p style='color: #666; margin-top: 20px;'>إذا لم تقم بهذا الطلب، يمكنك تجاهل هذا البريد بأمان.</p>
                </div>";

            await SendEmailAsync(email, subject, body, CancellationToken.None);
        }

        public async Task SendEmailAsync(string to, string subject, string body, CancellationToken cancellationToken = default)
        {
            var emailSettings = _config.GetSection("EmailSettings");
            var email = new MimeMessage();

            email.From.Add(new MailboxAddress(emailSettings["SenderName"] ?? "Identity Platform", emailSettings["SenderEmail"] ?? emailSettings["Username"]));
            email.To.Add(MailboxAddress.Parse(to));
            email.Subject = subject;

            var builder = new BodyBuilder { HtmlBody = body };
            email.Body = builder.ToMessageBody();

            using var smtp = new MailKit.Net.Smtp.SmtpClient();
            try
            {
                // استخدام المفاتيح السابقة تماماً كما كانت في كودك القديم
                await smtp.ConnectAsync(
                    emailSettings["SmtpServer"] ?? emailSettings["Host"],
                    int.Parse(emailSettings["Port"] ?? "587"),
                    MailKit.Security.SecureSocketOptions.StartTls,
                    cancellationToken);

                // المصادقة باستخدام البريد وكلمة المرور (أو الـ App Password)
                await smtp.AuthenticateAsync(
                    emailSettings["SenderEmail"] ?? emailSettings["Username"],
                    emailSettings["Password"],
                    cancellationToken);

                await smtp.SendAsync(email, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {To} with subject {Subject}", to, subject);
                throw;
            }
            finally
            {
                try
                {
                    await smtp.DisconnectAsync(true, cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error while disconnecting SMTP client");
                }
            }
        }

        //public async Task SendEmailAsync(string to, string subject, string body, CancellationToken cancellationToken)
        //{
        //    var smtpHost = _config["EmailSettings:Host"] ?? "smtp.gmail.com";
        //    var smtpPort = int.Parse(_config["EmailSettings:Port"] ?? "587");
        //    var smtpUser = _config["EmailSettings:Username"];
        //    var smtpPass = _config["EmailSettings:Password"];
        //    var senderEmail = _config["EmailSettings:SenderEmail"] ?? smtpUser;

        //    using var client = new SmtpClient(smtpHost, smtpPort)
        //    {
        //        // [مهم جداً] تفعيل المصادقة باستخدام بيانات البريد
        //        Credentials = new NetworkCredential(smtpUser, smtpPass),
        //        EnableSsl = true // تفعيل الاتصال الآمن المطلوبة من قبل السيفرات الحديثة
        //    };

        //    var mailMessage = new MailMessage
        //    {
        //        From = new MailAddress(senderEmail!),
        //        Subject = subject,
        //        Body = body,
        //        IsBodyHtml = true
        //    };

        //    mailMessage.To.Add(to);

        //    await client.SendMailAsync(mailMessage, cancellationToken);
        //}
    }
}

