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
            var from = new EmailAddress(Constants.Email.CustomerCreated.fromEmail, "Example User");
            var subject = Constants.Email.CustomerCreated.subject;
            var to = new EmailAddress(Constants.Email.CustomerCreated.toEmail, "Example User");
            var plainTextContent = Constants.Email.CustomerCreated.content;
            var htmlContent = $"<strong>{Constants.Email.CustomerCreated.content}</strong>";
            var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);

            var response = await emailClient.SendEmailAsync(msg);
            return true;
        }
        catch (Exception ex)
        {
            return false;
        }
    }

    public async Task<bool> SendCustomerCreationFailedEmail(CustomerModel customerModel)
    {
        try
        {
            var emailClient = new SendGridClient(_sendGridKey);
            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
    
}