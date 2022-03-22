using System.Security.Cryptography;
using Azure.Core.Pipeline;
using Customer.POC.Clients.Abstractions;
using Customer.POC.Models;
using Customer.POC.Settings;
using Microsoft.Extensions.Options;

namespace Customer.POC.Clients;

public class CustomerCreationClient : ICustomerCreationClient
{
    private readonly string _baseUrl;

    public CustomerCreationClient(IOptions<Settings.Settings> settings)
    {
        var configValue = settings.Value;
        _baseUrl = configValue.CustomerCreationApiBaseUrl;

    }
    public string CreateCustomer(CustomerModel request)
    {
    
        return _baseUrl;
    }
    
}