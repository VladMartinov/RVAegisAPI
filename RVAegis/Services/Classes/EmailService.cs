using MailKit.Net.Smtp;
using Microsoft.Extensions.Options;
using MimeKit;
using RVAegis.Helpers;
using RVAegis.Services.Interfaces;

namespace RVAegis.Services.Classes
{
    public class EmailService(IOptions<SmtpSettings> smtpSettings) : IEmailService
    {
        private readonly SmtpSettings _smtpSettings = smtpSettings.Value;

        public async Task SendPasswordResetEmailAsync(string email, string resetLink)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_smtpSettings.FromName, _smtpSettings.FromAddress));
            message.To.Add(new MailboxAddress("", email));
            message.Subject = "Восстановление пароля";

            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = $@"
                <h1>Восстановление пароля</h1>
                <p>Для сброса пароля перейдите по ссылке:</p>
                <a href='{resetLink}'>{resetLink}</a>
                <p>Ссылка действительна 1 час.</p>"
            };

            message.Body = bodyBuilder.ToMessageBody();

            using var client = new SmtpClient();
            await client.ConnectAsync(_smtpSettings.Server, _smtpSettings.Port, _smtpSettings.UseSsl);
            await client.AuthenticateAsync(_smtpSettings.Username, _smtpSettings.Password);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }
    }
}
