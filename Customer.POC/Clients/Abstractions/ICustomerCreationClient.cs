using Customer.POC.Models;

namespace Customer.POC.Clients.Abstractions;

public interface ICustomerCreationClient
{
    public string CreateCustomer(CustomerModel request);
}