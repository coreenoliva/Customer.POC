using System;
using System.Net;
using System.Threading;
using Customer.POC;
using Customer.POC.Clients;
using Customer.POC.Models;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Serialization.HybridRow;
using Moq;
using Shouldly;
using Test.TestData;
using Xunit;

namespace Test;

public class CustomerDatabaseClientTests
{
    [Fact]
    public void CreateCustomer_ValidCustomerModel_CreatesCustomerInCosmosSuccessfully()
    {
        // Arrange 
        var mockCosmosClient = new Mock<CosmosClient>();
        var mockContainer = new Mock<Container>();
        var responseMock = new Mock<ItemResponse<CustomerModelCosmos>>();
        responseMock
            .Setup(x => x.StatusCode)
            .Returns(HttpStatusCode.Created);
        var input = CustomerModelTestData.Default;
        
        mockContainer.Setup(x => x
            .CreateItemAsync(
                It.IsAny<CustomerModelCosmos>(), 
                null, 
                null, 
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(responseMock.Object);
        mockCosmosClient.Setup(x => x
            .GetContainer(Constants.CosmosDb.databaseName, Constants.CosmosDb.containerName))
            .Returns(mockContainer.Object);
        var databaseClient = new CustomerDatabaseClient(mockCosmosClient.Object);

        // Act
        var response = databaseClient.CreateCustomer(input);

        // Assert
        response.ShouldNotBeNull();
        var guidParseResult = Guid.TryParse(response.Result, out var guidParse);
        guidParseResult.ShouldBe(true);
    }
    
    [Fact]
        public void CreateCustomer_CosmosError_ReturnsNull()
        {
            // Arrange 
            var mockCosmosClient = new Mock<CosmosClient>();
            var mockContainer = new Mock<Container>();
            var responseMock = new Mock<ItemResponse<CustomerModelCosmos>>();
            responseMock
                .Setup(x => x.StatusCode)
                .Returns(HttpStatusCode.InternalServerError);
            var input = CustomerModelTestData.Default;
            
            mockContainer.Setup(x => x
                .CreateItemAsync(
                    It.IsAny<CustomerModelCosmos>(), 
                    null, 
                    null, 
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(responseMock.Object);
            mockCosmosClient.Setup(x => x
                .GetContainer(Constants.CosmosDb.databaseName, Constants.CosmosDb.containerName))
                .Returns(mockContainer.Object);
            var databaseClient = new CustomerDatabaseClient(mockCosmosClient.Object);
    
            // Act
            var response = databaseClient.CreateCustomer(input);
    
            // Assert
            response.Result.ShouldBeNull();
        }
}