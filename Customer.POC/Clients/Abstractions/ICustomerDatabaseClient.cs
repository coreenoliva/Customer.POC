using System.Threading.Tasks;
using Customer.POC.Models;

namespace Customer.POC.Clients.Abstractions;

public interface ICustomerDatabaseClient
{
    public Task<bool> CreateCustomer(CustomerModel customer);
}