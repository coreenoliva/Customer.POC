using System.Threading.Tasks;
using Customer.POC.Models;
using SendGrid;

namespace Customer.POC.Clients.Abstractions;

public interface IEmailClient
{
    public Task<bool> SendCustomerCreatedEmail(CustomerModel customerModel);
}