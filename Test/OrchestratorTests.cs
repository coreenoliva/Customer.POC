using System;
using Customer.POC;
using Customer.POC.Clients;
using Customer.POC.Clients.Abstractions;
using Customer.POC.Models;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;
using Moq;
using Shouldly;
using Test.TestData;
using Xunit;

namespace Test;

public class OrchestratorTests
{
    private readonly  Mock<ICustomerCreationClient> _mockCustomerCreationClient;
    private readonly  Mock<ICustomerDatabaseClient> _mockCustomerDatabaseClient;
    private readonly  Mock<IEmailClient> _mockEmailClient;
    private readonly  Mock<ILogger> _mockLogger;
    private readonly  Mock<AbstractValidator<CustomerModel>> _mockValidator;
    
    public OrchestratorTests()
    {
        _mockCustomerCreationClient = new Mock<ICustomerCreationClient>();
        _mockCustomerDatabaseClient = new Mock<ICustomerDatabaseClient>();
        _mockEmailClient = new Mock<IEmailClient>();
        _mockLogger = new Mock<ILogger>();
        _mockValidator = new Mock<AbstractValidator<CustomerModel>>();
    }
    [Fact]
    public void ValidateRequest_ValidInput_ReturnsTrue()
    {
        var mockResult = new Mock<ValidationResult>();

        // Arrange
        mockResult.Setup(x => x.IsValid).Returns(true);
        
        _mockValidator.Setup(x => x.Validate(It.IsAny<ValidationContext<CustomerModel>>()))
                    .Returns(mockResult.Object);
        
        var orchestrator = new Orchestrator(_mockCustomerCreationClient.Object,
                                            _mockCustomerDatabaseClient.Object,
                                            _mockEmailClient.Object,
                                            _mockValidator.Object);
        // Act
        var response = orchestrator.ValidateRequest(CustomerModelTestData.Default, _mockLogger.Object);
        
        // Assert
        response.ShouldBe(true);
    }
    
    [Fact]
    public void ValidateRequest_InvalidInput_ReturnsFalse()
    {
        // Arrange
        var mockResult = new Mock<ValidationResult>();
        mockResult.Setup(x => x.IsValid).Returns(false);
        
        _mockValidator.Setup(x => x.Validate(It.IsAny<ValidationContext<CustomerModel>>()))
            .Returns(mockResult.Object);
        
        var orchestrator = new Orchestrator(_mockCustomerCreationClient.Object,
            _mockCustomerDatabaseClient.Object,
            _mockEmailClient.Object,
            _mockValidator.Object);
        // Act
        var response = orchestrator.ValidateRequest(CustomerModelTestData.MissingCountry, _mockLogger.Object);
        
        // Assert
        response.ShouldBe(false);
    }

    [Fact]
    public void SendCustomerEmail_EmailSentSuccessfully_ReturnsTrue()
    {
        // Arrange
        var request = CustomerModelTestData.Default;
        _mockEmailClient.Setup(x => x.SendCustomerCreatedEmail(request))
            .ReturnsAsync(true);
        
        var orchestrator = new Orchestrator(_mockCustomerCreationClient.Object,
            _mockCustomerDatabaseClient.Object,
            _mockEmailClient.Object,
            _mockValidator.Object);

        // Act
        var response = orchestrator.SendCustomerEmail(request, _mockLogger.Object);

        // Assert
        response.Result.ShouldBe(true);
    }
    
    [Fact]
    public void SendCustomerEmail_ErrorWithEmailSend_ReturnsFalse()
    {

        // Arrange
        var request = CustomerModelTestData.Default;
        _mockEmailClient.Setup(x => x.SendCustomerCreatedEmail(request))
            .ReturnsAsync(false);
        
        var orchestrator = new Orchestrator(_mockCustomerCreationClient.Object,
            _mockCustomerDatabaseClient.Object,
            _mockEmailClient.Object,
            _mockValidator.Object);

        // Act
        var response = orchestrator.SendCustomerEmail(request, _mockLogger.Object);

        // Assert
        response.Result.ShouldBe(false);
    }

    [Fact]
    public void CreateCustomerThirdParty_SuccessfullyCreatesCustomer_ReturnsTrue()
    {
        throw new NotImplementedException();
 
    }
    
    [Fact]
    public void CreateCustomerThirdParty_ErrorCreatingCustomer_ReturnsFalse()
    {
        throw new NotImplementedException();
 
    }
    
    [Fact]
    public void CreateCustomerCosmosDb_SuccessfullyCreatesCustomer_ReturnsTrue()
    {
        throw new NotImplementedException();
 
    }
    
    [Fact]
    public void CreateCustomerCosmosDb_ErrorCreatingCustomer_ReturnsFalse()
    {
        throw new NotImplementedException();
 
    }
    
    [Fact]
    public void RunOrchestrator_NoErrors_AllCallsSuccessfull()
    {
        throw new NotImplementedException();
    }
    
    [Fact]
    public void RunOrchestrator_ActivityValidatorReturnsFalse_ExitsAfterValidationCall()
    {
        throw new NotImplementedException();
    }
    
    [Fact]
    public void RunOrchestrator_ActivityCreateCustomerThirdPartyReturnsFalse_ExitsAfter()
    {
        throw new NotImplementedException();
    }
    
    [Fact]
    public void RunOrchestrator_ActivityCreateCustomerCosmosDbReturnsFalse_ExitsAfter()
    {
        throw new NotImplementedException();
    }
    
    [Fact]
    public void RunOrchestrator_ActivitySEndEmailCustomerReturnsFalse_ExitsAfter()
    {
        throw new NotImplementedException();
    }
    
    [Fact]
    public void RunOrchestrator_UnhandledExceptionErrorOccurs_ExitsAfter()
    {
        throw new NotImplementedException();
    }

    


}