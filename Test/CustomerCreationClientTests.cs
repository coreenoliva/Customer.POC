using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Customer.POC.Clients;
using Customer.POC.Settings;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using Test.TestData;
using Xunit;

namespace Test;

public class CustomerCreationClientTests
{
    [Fact]
    public void CreateCustomerAsync_ValidCustomerModel_CreatesCustomerSuccessfully()
    {
        // Arrange
        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        
        mockHttpMessageHandler.
            Protected().
            Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage{StatusCode = HttpStatusCode.OK});
        var httpClient = new HttpClient(mockHttpMessageHandler.Object); 
        var customerCreationClient = new CustomerCreationClient(OptionsSettingsTestData.DefaultSettings, httpClient);
        
        // Act
        var result = customerCreationClient.CreateCustomerAsync(CustomerModelTestData.Default);

        // Assert 
        mockHttpMessageHandler.Protected().Verify
        ("SendAsync", 
            Times.Exactly(1), 
            ItExpr.Is<HttpRequestMessage>(x => 
                x.Method == HttpMethod.Post
                && x.RequestUri == new Uri(
                    OptionsSettingsTestData.DefaultSettings.Value.CustomerCreationApiBaseUrl,   
                    "customer")), ItExpr.IsAny<CancellationToken>());
    }
        
        
}