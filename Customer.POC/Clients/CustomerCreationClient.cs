using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Customer.POC.Clients.Abstractions;
using Customer.POC.Models;
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
        using var client = new HttpClient();
        var path = "customer";
        var fullPath = new Uri(_baseUrl, path);
        var result = await client.PostAsync(fullPath, content);
        return result.IsSuccessStatusCode;
    }
}