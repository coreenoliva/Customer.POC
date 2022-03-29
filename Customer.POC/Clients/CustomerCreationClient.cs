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
    private readonly HttpClient _httpClient;

    public CustomerCreationClient(IOptions<Settings.Settings> settings, HttpClient httpClient)
    {
        var configValue = settings.Value;
        _baseUrl = configValue.CustomerCreationApiBaseUrl;
        _httpClient = httpClient;
    }

    public async Task<bool> CreateCustomerAsync(CustomerModel request)
    {
        var payload = JsonConvert.SerializeObject(request);

        HttpContent content = new StringContent(payload, Encoding.UTF8, "application/json");
        var path = "customer";
        var fullPath = new Uri(_baseUrl, path);
        var result = await _httpClient.PostAsync(fullPath, content);
        return result.IsSuccessStatusCode;
    }
}