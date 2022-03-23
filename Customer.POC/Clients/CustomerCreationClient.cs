using System;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Azure.Core.Pipeline;
using Customer.POC.Clients.Abstractions;
using Customer.POC.Models;
using Customer.POC.Settings;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Customer.POC.Clients;

public class CustomerCreationClient : ICustomerCreationClient
{
    private readonly Uri _baseUrl;

    public CustomerCreationClient(IOptions<Settings.Settings> settings)
    {
        var configValue = settings.Value;
        _baseUrl = configValue.CustomerCreationApiBaseUrl;
    }
    public async Task<bool> CreateCustomerAsync(CustomerModel request)
    {
        var payload = JsonConvert.SerializeObject(request);
        
        HttpContent content = new StringContent(payload, Encoding.UTF8, "application/json");
        using (var client = new HttpClient())
        {
            var path = "customer";
            Uri fullPath = new Uri(_baseUrl, path);
            HttpResponseMessage result = await client.PostAsync(fullPath, content);

            if (result.IsSuccessStatusCode)
            {
                return true;
            }
            else
            {
             // todo error handling/logging   
            }
        }

        return false;
    }
    
}