using MailKit.Net.Smtp;
using MimeKit;
using System.Threading.Tasks;

namespace ApiSkyrimRP.Core
{
    public class MailService
    {
        private readonly string MailFrom;
        private readonly string MailAddress;
        private readonly string MailPassword;

        private readonly string MailServer;
        private readonly ushort MailServerPort;

        public readonly bool Enable = false;

        public MailService(string mailFrom, string mailAddress, string mailPassword, string mailServer, ushort mailServerPort)
        {
            MailFrom = mailFrom;
            MailAddress = mailAddress;
            MailPassword = mailPassword;
            MailServer = mailServer;
            MailServerPort = mailServerPort;

            Enable = !string.IsNullOrEmpty(MailAddress) && !string.IsNullOrEmpty(MailPassword);
        }

        public async Task SendMailAsync(string email, string subject, string message)
        {
            if (!Enable) return;

            MimeMessage emailMessage = new();

            emailMessage.From.Add(new MailboxAddress(MailFrom, MailAddress));
            emailMessage.To.Add(new MailboxAddress("", email));
            emailMessage.Subject = subject;
            emailMessage.Body = new TextPart(MimeKit.Text.TextFormat.Html)
            {
                Text = message
            };

            using SmtpClient client = new();
            client.CheckCertificateRevocation = false;
            client.SslProtocols = System.Security.Authentication.SslProtocols.Tls;
            await client.ConnectAsync(MailServer, MailServerPort, true);
            await client.AuthenticateAsync(MailAddress, MailPassword);
            await client.SendAsync(emailMessage);

            await client.DisconnectAsync(true);
        }
    }
}
