using System.Net;
using System.Net.Http;
using System.Threading;
using Customer.POC.Clients;
using Moq;
using SendGrid;
using SendGrid.Helpers.Mail;
using Shouldly;
using Test.TestData;
using Xunit;

namespace Test;

public class EmailClientTests
{
    [Fact]
    public void SendCustomerCreatedEmail_EmailSentSuccessfully_ReturnsTrue()
    {
        // Arrange 
        var sendGridClientMock = new Mock<ISendGridClient>();
        var mockHttpContent = new Mock<HttpContent>();
        var sendGridResponse = new Response(HttpStatusCode.OK, mockHttpContent.Object, null);
        
        sendGridClientMock
            .Setup(x => x.SendEmailAsync(
                It.IsAny<SendGridMessage>(), 
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(sendGridResponse);
        
        var emailClient = new EmailClient(OptionsSettingsTestData.DefaultSettings, sendGridClientMock.Object);
        
        // Act
        var response = emailClient.SendCustomerCreatedEmail(CustomerModelTestData.Default);

        // Assert
        response.Result.ShouldBe(true);
    }

    [Fact]
    public void SendCustomerCreatedEmail_EmailFailedToSend_ReturnsFalse()
    {
        // Arrange 
        var sendGridClientMock = new Mock<ISendGridClient>();
        var mockHttpContent = new Mock<HttpContent>();
        var sendGridResponse = new Response(HttpStatusCode.InternalServerError, mockHttpContent.Object, null);
        
        sendGridClientMock
            .Setup(x => x.SendEmailAsync(
                It.IsAny<SendGridMessage>(), 
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(sendGridResponse);
        
        var emailClient = new EmailClient(OptionsSettingsTestData.DefaultSettings, sendGridClientMock.Object);
        
        // Act
        var response = emailClient.SendCustomerCreatedEmail(CustomerModelTestData.Default);

        // Assert
        response.Result.ShouldBe(false);
        
    } 
}