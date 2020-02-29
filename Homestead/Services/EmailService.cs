using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Net.Mail;

namespace Homestead.Services
{
    public class EmailService : IDisposable
    {
        private SmtpClient client;
        private ILogger log;

        private readonly string apikey;
        private readonly string secretkey;
        private readonly string htmlTemplate;

        public EmailService(string apikey, string secretkey, string htmlTemplate, ILoggerFactory factory)
        {
            this.apikey = apikey;
            this.secretkey = secretkey;
            this.htmlTemplate = htmlTemplate;
            client = new SmtpClient("in.mailjet.com", 587);
            log = factory.CreateLogger<EmailService>();
        }

        public void SendMessageAsync(User to, int estimate, int low, int high)
        {
            MailMessage msg = new MailMessage
            {
                From = new MailAddress("austin@harmonize.azurewebsites.net")
            };

            msg.To.Add(new MailAddress(to.Email));
            msg.Subject = "Welcome to Harmonize";
            msg.Body = string.Format(htmlTemplate, to.Name, estimate.ToString("C"), low.ToString("C"), high.ToString("C"), to.IP);
            msg.IsBodyHtml = true;

            client.DeliveryMethod = SmtpDeliveryMethod.Network;
            client.EnableSsl = true;
            client.UseDefaultCredentials = false;
            client.Credentials = new NetworkCredential(apikey, secretkey);

            try
            {
                client.SendAsync(msg, null);
            }
            catch (Exception ex)
            {
                log.LogCritical(ex, "Failed to send email");
            }
        }

        public void Dispose()
        {
            if (client != null)
            {
                client.Dispose();
                client = null;
            }
        }
    }
}
