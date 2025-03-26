using System.Net;
using System.Net.Mail;

namespace WebApplication1.Services
{
    public class EmailService
    {
        public Task SendEmail(string to, string subject, string body)
        {
            var mail = "v.y.shumsjkyy@student.khai.edu";
            var pw = "olmh llbn zfwk bnyy"; 

            var client = new SmtpClient("smtp.gmail.com", 587)
            {
                EnableSsl = true,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(mail, pw)
            };

            var message = new MailMessage(mail, to, subject, body);
            message.IsBodyHtml = true; 

            return client.SendMailAsync(message);
        }
    }
}
