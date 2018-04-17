using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;
using translate.web.Resources;

namespace translate.web.Services
{
    public class EmailSender : IEmailSender
    {
        private readonly LocService _locService;

        public Emailoptions Options { get; }

        public EmailSender(IOptions<Emailoptions> optionsAccessor, LocService locService)
        {
            Options = optionsAccessor.Value;
            _locService = locService;
        }

        public Task SendEmailAsync(string email, string subject, string message)
        {
            return Execute(Options.SendgridApiKey, subject, message, email);
        }

        private Task Execute(string apiKey, string subject, string message, string email)
        {
            var client = new SendGridClient(apiKey);
            var msg = new SendGridMessage()
            {
                From = new EmailAddress("Info@vs.com", _locService.GetLocalizedHtmlString("information")),
                Subject = subject,
                PlainTextContent = message,
                HtmlContent = message
            };

            msg.AddTo(new EmailAddress(email));
            return client.SendEmailAsync(msg);
        }
    }
}
