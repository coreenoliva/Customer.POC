using System;
using System.Threading.Tasks;
using Customer.POC.Clients.Abstractions;
using Customer.POC.Models;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Options;

namespace Customer.POC.Clients;

public class CustomerDatabaseClient : ICustomerDatabaseClient
{
    private readonly Uri _cosmosConnectionString;
    private readonly string _cosmosAccessKey;

    public CustomerDatabaseClient(IOptions<Settings.Settings> settings)
    {
        _cosmosConnectionString = settings.Value.CosmosDbConnectionString;
        _cosmosAccessKey = settings.Value.CosmosAccessKey;
    }

    public async Task<bool> CreateCustomer(CustomerModel customer)
    {
        var cosmosClient = new CosmosClient(_cosmosConnectionString.ToString(), _cosmosAccessKey);
        var container = cosmosClient.GetContainer(Constants.CosmosDb.databaseName, Constants.CosmosDb.containerName);
        
        // create new customer 
        var cosmosDbModel = new CustomerModelCosmos().CreateCustomer(customer);
        
        // check if customer exists
        
        // if not, create
        return false;
    }
}