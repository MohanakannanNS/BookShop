using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utility
{
    public class EmailSender : IEmailSender
    {
        private readonly EmailOptions _emailOptions;
        public EmailSender(IOptions<EmailOptions> options)
        {
            _emailOptions = options.Value;
        }
        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            return Execute(_emailOptions.SendGridKey, subject, htmlMessage,email);
        }
        private Task Execute(string key,string subject, string html,string email)
        {
            var client = new SendGridClient(key);
            var from = new EmailAddress("admin@bookshop.com", "End User");
            var to = new EmailAddress(email, "End User");
            var msg = MailHelper.CreateSingleEmail(from, to, subject, "", html);
            return client.SendEmailAsync(msg);
        }
    }
}
