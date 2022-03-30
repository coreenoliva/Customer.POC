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
    private readonly CosmosClient _cosmosClient;

    public CustomerDatabaseClient(CosmosClient cosmosClient)
    {
        _cosmosClient = cosmosClient;
    }

    public async Task<string> CreateCustomer(CustomerModel customer)
    {
        var container = _cosmosClient.GetContainer(Constants.CosmosDb.databaseName, Constants.CosmosDb.containerName);

        //todo - maybe dupe check? 
        var cosmosDbModel = new CustomerModelCosmos().CreateCustomer(customer);

        var itemCreationResult = await container.CreateItemAsync(cosmosDbModel);
        if (itemCreationResult.StatusCode == HttpStatusCode.Created)
        {
            return cosmosDbModel.Id;
        }
        return null;
    }
}