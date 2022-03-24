using System;
using System.Threading.Tasks;
using Customer.POC.Clients.Abstractions;
using Customer.POC.Models;
using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace Customer.POC.Clients;

public class EmailClient : IEmailClient
{
    private readonly string _sendGridKey;

    public EmailClient(IOptions<Settings.Settings> settings)
    {
        _sendGridKey = settings.Value.SendGridKey;
    }

    public async Task<bool> SendCustomerCreatedEmail(CustomerModel customerModel)
    {
        try
        {
            var emailClient = new SendGridClient(_sendGridKey);
            var from = new EmailAddress("coreen.oliva@bjss.com", "Example User");
            var subject = "Sending with SendGrid is Fun";
            var to = new EmailAddress("coreenoliva@gmail.com", "Example User");
            var plainTextContent = "and easy to do anywhere, even with C#";
            var htmlContent = "<strong>and easy to do anywhere, even with C#</strong>";
            var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);

            var response = await emailClient.SendEmailAsync(msg);
            return true;
        }
        catch (Exception ex)
        {
            return false;
            
        }

        return false;
    }
}