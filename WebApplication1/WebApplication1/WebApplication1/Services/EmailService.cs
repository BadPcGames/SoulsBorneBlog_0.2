using MailKit.Security;
using MimeKit;
using MimeKit.Text;
using System.Net;
using System.Net.Mail;
using MailKit.Net.Smtp;

namespace WebApplication1.Services
{
    public class EmailService
    {
        private readonly string _smtpServer = "smtp.gmail.com";
        private readonly int _smtpPort = 587;

        // Ensure credentials are securely retrieved, not hardcoded
        private readonly string _email = "v.y.shumsjkyy@student.khai.edu";
        private readonly string _password = "olmh llbn zfwk bnyy";

        public async Task SendEmail(string to, string subject, string body)
        {
            var client = new System.Net.Mail.SmtpClient(_smtpServer, _smtpPort)
            {
                EnableSsl = true,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(_email, _password)
            };

            var message = new MailMessage(_email, to, subject, body)
            {
                IsBodyHtml = true
            };

            await client.SendMailAsync(message);
        }

        public async Task SendHtmlEmailAsync(string toEmail, string subject, string htmlContent)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("Site Admin", _email)); // Change to your site admin email
            message.To.Add(new MailboxAddress("", toEmail));
            message.Subject = subject;
            message.Body = new TextPart(TextFormat.Html) { Text = htmlContent };

            using (var client = new MailKit.Net.Smtp.SmtpClient())
            {
                await client.ConnectAsync("smtp.gmail.com", 587, SecureSocketOptions.StartTls);
                await client.AuthenticateAsync(_email, _password);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);
            }
        }
    }

}
