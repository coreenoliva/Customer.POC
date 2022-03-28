using System;
using System.Net;
using System.Threading.Tasks;
using Customer.POC.Clients.Abstractions;
using Customer.POC.Models;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Options;

namespace Customer.POC.Clients;

public class CustomerDatabaseClient : ICustomerDatabaseClient
{
    private readonly string _cosmosAccessKey;
    private readonly Uri _cosmosConnectionString;

    public CustomerDatabaseClient(IOptions<Settings.Settings> settings)
    {
        _cosmosConnectionString = settings.Value.CosmosDbConnectionString;
        _cosmosAccessKey = settings.Value.CosmosAccessKey;
    }

    public async Task<bool> CreateCustomer(CustomerModel customer)
    {
        var cosmosClient = new CosmosClient(_cosmosConnectionString.ToString());
        var container = cosmosClient.GetContainer(Constants.CosmosDb.databaseName, Constants.CosmosDb.containerName);

        //todo - maybe dupe check? 
        var cosmosDbModel = new CustomerModelCosmos().CreateCustomer(customer);

        try
        {
            var itemCreationResult = await container.CreateItemAsync(cosmosDbModel);
            if (itemCreationResult.StatusCode == HttpStatusCode.Created) return true;
        }
        catch (Exception ex)
        {
            var error = ex;
        }

        return false;
    }
}