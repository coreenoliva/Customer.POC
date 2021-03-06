using System.Threading.Tasks;
using Customer.POC.Models;

namespace Customer.POC.Clients.Abstractions;

public interface ICustomerCreationClient
{
    public Task<bool> CreateCustomerAsync(CustomerModel request);
}