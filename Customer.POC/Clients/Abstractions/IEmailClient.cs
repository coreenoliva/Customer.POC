using System.Threading.Tasks;
using Customer.POC.Models;

namespace Customer.POC.Clients.Abstractions;

public interface IEmailClient
{
    public Task<bool> SendCustomerCreatedEmail(CustomerModel customerModel);
    public Task<bool> SendCustomerCreationFailedEmail(CustomerModel customerModel);
}