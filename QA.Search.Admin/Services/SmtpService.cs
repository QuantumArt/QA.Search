using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

namespace QA.Search.Admin.Services
{
    public class SmtpService
    {
        private SmtpServiceSettings Settings { get; }

        public SmtpService(IOptions<SmtpServiceSettings> settings)
        {
            Settings = settings.Value;
        }

        public async Task Send(string address, string subject, string content)
        {
            
            using (var smtpClient = new SmtpClient {
                Host = Settings.Host,
                Port = Settings.Port,
                EnableSsl = Settings.EnableSsl,
                UseDefaultCredentials = Settings.UseDefaultCredentials,
                DeliveryMethod = SmtpDeliveryMethod.Network
            })
            {
                if (!Settings.UseDefaultCredentials)
                {
                    smtpClient.Credentials = new NetworkCredential(Settings.User, Settings.Password);
                }

                using (var message = new MailMessage(
                    new MailAddress(Settings.From, Settings.DisplayName),
                    new MailAddress(address))
                )
                {
                    message.Subject = subject;
                    message.Body = content;
                    message.IsBodyHtml = true;

                    //TODO handle error when sending msg

                    await smtpClient.SendMailAsync(message);
                }

            }
        }
    }
}
