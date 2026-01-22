using System.Net.Mail; // for SmtpException
using EShop.Domain;
using EShop.Domain.DomainModels;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace EShop.Service
{
    public class EmailService : IEmailService
    {
        private readonly MailSettings _mailSettings;

        public EmailService(IOptions<MailSettings> mailSettings)
        {
            _mailSettings = mailSettings.Value;
        }

        public bool SendEmailAsync(EmailMessage message)
        {
            var emailMessage = new MimeMessage
            {
                Sender = new MailboxAddress(
                    _mailSettings.SendersName,
                    _mailSettings.SmtpUserName),
                Subject = message.Subject ?? string.Empty
            };

            emailMessage.From.Add(new MailboxAddress(
                _mailSettings.EmailDisplayName,
                _mailSettings.SmtpUserName));

            emailMessage.To.Add(new MailboxAddress(
                message.MailTo ?? string.Empty,
                message.MailTo ?? string.Empty));

            emailMessage.Body = new TextPart(MimeKit.Text.TextFormat.Plain)
            {
                Text = message.Content ?? string.Empty
            };

            try
            {
                using var smtp = new MailKit.Net.Smtp.SmtpClient();

                var socketOptions = _mailSettings.EnableSsl
                    ? SecureSocketOptions.StartTls
                    : SecureSocketOptions.Auto;

                smtp.Connect(
                    _mailSettings.SmtpServer,
                    _mailSettings.SmtpServerPort,
                    socketOptions);

                if (!string.IsNullOrEmpty(_mailSettings.SmtpUserName))
                {
                    smtp.Authenticate(
                        _mailSettings.SmtpUserName,
                        _mailSettings.SmtpPassword);
                }

                smtp.Send(emailMessage);
                smtp.Disconnect(true);

                return true;
            }
            catch (SmtpException)
            {
                // if sending fails, just report false, don't crash app
                return false;
            }
        }
    }
}
