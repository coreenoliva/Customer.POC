using System;
using System.Threading.Tasks;
using Customer.POC;
using Customer.POC.Clients;
using Customer.POC.Clients.Abstractions;
using Customer.POC.Models;
using Customer.POC.Validators;
using FluentValidation;
using FluentValidation.Internal;
using FluentValidation.Results;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
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
    // private readonly  Mock<AbstractValidator<CustomerModel>> _mockValidator;
    private readonly InputValidator _validator;
    
    public OrchestratorTests()
    {
        _mockCustomerCreationClient = new Mock<ICustomerCreationClient>();
        _mockCustomerDatabaseClient = new Mock<ICustomerDatabaseClient>();
        _mockEmailClient = new Mock<IEmailClient>();
        _mockLogger = new Mock<ILogger>();
        _validator = new InputValidator();
        // _mockValidator = new Mock<AbstractValidator<CustomerModel>>();
    }
    [Fact]
    public void ValidateRequest_ValidInput_ReturnsTrue()
    {
        // Arrange
        var mockResult = new Mock<ValidationResult>();
        var request = CustomerModelTestData.Default;
        mockResult.Setup(x => x.IsValid).Returns(true);

        var orchestrator = new Orchestrator(_mockCustomerCreationClient.Object,
                                            _mockCustomerDatabaseClient.Object,
                                            _mockEmailClient.Object,
                                            _validator);
        // Act
        var response = orchestrator.ValidateRequest(request, _mockLogger.Object);
        
        // Assert
        response.ShouldBe(true);
    }
    
    [Fact]
    public void ValidateRequest_InvalidInput_ReturnsFalse()
    {
        // Arrange
        var mockResult = new Mock<ValidationResult>();
        var request = CustomerModelTestData.MissingCountry;

        mockResult.Setup(x => x.IsValid).Returns(false);
        
        var orchestrator = new Orchestrator(_mockCustomerCreationClient.Object,
            _mockCustomerDatabaseClient.Object,
            _mockEmailClient.Object,
            _validator);
        
        // Act
        var response = orchestrator.ValidateRequest(request, _mockLogger.Object);
        
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
            _validator);

        // Act
        var response = orchestrator.SendCustomerEmail(request, _mockLogger.Object);

        // Assert
        response.Result.ShouldBe(true);
        _mockEmailClient.Verify(x => x.SendCustomerCreatedEmail(request), Times.Once);
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
            _validator);

        // Act
        var response = orchestrator.SendCustomerEmail(request, _mockLogger.Object);

        // Assert
        response.Result.ShouldBe(false);
        _mockEmailClient.Verify(x => x.SendCustomerCreatedEmail(request), Times.Once);
    }

    [Fact]
    public void CreateCustomerThirdParty_SuccessfullyCreatesCustomer_ReturnsTrue()
    {
        // Arrange
        var request = CustomerModelTestData.Default;
        _mockCustomerCreationClient.Setup(x => x.CreateCustomerAsync(request))
            .ReturnsAsync(true);
        
        var orchestrator = new Orchestrator(_mockCustomerCreationClient.Object,
            _mockCustomerDatabaseClient.Object,
            _mockEmailClient.Object,
            _validator);
        
        // Act
        var response = orchestrator.CreateCustomerThirdParty(request, _mockLogger.Object);
        
        // Assert
        response.Result.ShouldBe(true);
        _mockCustomerCreationClient.Verify(x => x.CreateCustomerAsync(request), Times.Once);
    }
    
    [Fact]
    public void CreateCustomerThirdParty_ErrorCreatingCustomer_ReturnsFalse()
    {
        // Arrange
        var request = CustomerModelTestData.Default;
        _mockCustomerCreationClient.Setup(x => x.CreateCustomerAsync(request))
            .ReturnsAsync(false);
        
        var orchestrator = new Orchestrator(_mockCustomerCreationClient.Object,
            _mockCustomerDatabaseClient.Object,
            _mockEmailClient.Object,
            _validator);
        
        // Act
        var response = orchestrator.CreateCustomerThirdParty(request, _mockLogger.Object);
        
        // Assert
        response.Result.ShouldBe(false);
        _mockCustomerCreationClient.Verify(x => x.CreateCustomerAsync(request), Times.Once);
    }
    
    [Fact]
    public void CreateCustomerCosmosDb_SuccessfullyCreatesCustomer_ReturnsTrue()
    {
        // Arrange
        var request = CustomerModelTestData.Default;
        _mockCustomerDatabaseClient.Setup(x => x.CreateCustomer(request))
            .ReturnsAsync(Guid.NewGuid().ToString());
        
        var orchestrator = new Orchestrator(_mockCustomerCreationClient.Object,
            _mockCustomerDatabaseClient.Object,
            _mockEmailClient.Object,
            _validator);
        
        // Act
        var response = orchestrator.CreateCustomerCosmosDb(request, _mockLogger.Object);

        // Assert
        response.Result.ShouldBe(true);
        _mockCustomerDatabaseClient.Verify(x => x.CreateCustomer(request), Times.Once);
    }
    
    [Fact]
    public void CreateCustomerCosmosDb_ErrorCreatingCustomer_ReturnsFalse()
    {
        // Arrange
        var request = CustomerModelTestData.Default;
        _mockCustomerDatabaseClient.Setup(x => x.CreateCustomer(request))
            .Returns(Task.FromResult<string?>(null));
        
        var orchestrator = new Orchestrator(_mockCustomerCreationClient.Object,
            _mockCustomerDatabaseClient.Object,
            _mockEmailClient.Object,
            _validator);
        
        // Act
        var response = orchestrator.CreateCustomerCosmosDb(request, _mockLogger.Object);

        // Assert
        
        response.Result.ShouldBe(false);
        _mockCustomerDatabaseClient.Verify(x => x.CreateCustomer(request), Times.Once);
    }
    
    [Fact]
    public void RunOrchestrator_NoErrors_AllCallsSuccessful()
    {
        // Arrange
        var mockContext = new Mock<IDurableOrchestrationContext>();
        
        var orchestrator = new Orchestrator(_mockCustomerCreationClient.Object,
            _mockCustomerDatabaseClient.Object,
            _mockEmailClient.Object,
            _validator);
        
        // Act
        var response = orchestrator.RunOrchestrator(mockContext.Object, _mockLogger.Object);
        
        // Assert
        response.ShouldNotBeNull();
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