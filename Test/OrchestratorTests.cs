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
        var request = CustomerModelTestData.Default;

        mockContext.Setup(x => x.CallActivityAsync<bool>(Constants.ActivityNames.validator, It.IsAny<CustomerModel>()))
            .ReturnsAsync(true);
        
        
        mockContext.Setup(x => x.CallActivityAsync<bool>(Constants.ActivityNames.createCustomerThirdParty, It.IsAny<CustomerModel>()))
            .ReturnsAsync(true);
        
        mockContext.Setup(x => x.CallActivityAsync<bool>(Constants.ActivityNames.createCustomerCosmosDb, It.IsAny<CustomerModel>()))
            .ReturnsAsync(true);
        
        mockContext.Setup(x => x.CallActivityAsync<bool>(Constants.ActivityNames.sendEmailCustomer, It.IsAny<CustomerModel>()))
            .ReturnsAsync(true);
        
        var orchestrator = new Orchestrator(_mockCustomerCreationClient.Object,
            _mockCustomerDatabaseClient.Object,
            _mockEmailClient.Object,
            _validator);
        
        // Act
        var response = orchestrator.RunOrchestrator(mockContext.Object, _mockLogger.Object);
        
        // Assert
        response.ShouldNotBeNull();
        response.Result.Count.ShouldBe(0);
        mockContext.Verify(x => x.CallActivityAsync<bool>(Constants.ActivityNames.validator, It.IsAny<CustomerModel>()), Times.Once);
        mockContext.Verify(x => x.CallActivityAsync<bool>(Constants.ActivityNames.createCustomerThirdParty, It.IsAny<CustomerModel>()), Times.Once);
        mockContext.Verify(x => x.CallActivityAsync<bool>(Constants.ActivityNames.createCustomerCosmosDb, It.IsAny<CustomerModel>()), Times.Once);
        mockContext.Verify(x => x.CallActivityAsync<bool>(Constants.ActivityNames.sendEmailCustomer, It.IsAny<CustomerModel>()), Times.Once);
    }
    
    [Fact]
    public void RunOrchestrator_ActivityValidatorReturnsFalse_ExitsAfterValidationCall()
    {
        // Arrange
        var mockContext = new Mock<IDurableOrchestrationContext>();

        mockContext.Setup(x => x.CallActivityAsync<bool>(Constants.ActivityNames.validator, It.IsAny<CustomerModel>()))
            .ReturnsAsync(false);
        
        mockContext.Setup(x => x.CallActivityAsync<bool>(Constants.ActivityNames.createCustomerThirdParty, It.IsAny<CustomerModel>()))
            .ReturnsAsync(true);
        
        mockContext.Setup(x => x.CallActivityAsync<bool>(Constants.ActivityNames.createCustomerCosmosDb, It.IsAny<CustomerModel>()))
            .ReturnsAsync(true);
        
        mockContext.Setup(x => x.CallActivityAsync<bool>(Constants.ActivityNames.sendEmailCustomer, It.IsAny<CustomerModel>()))
            .ReturnsAsync(true);
        
        var orchestrator = new Orchestrator(_mockCustomerCreationClient.Object,
            _mockCustomerDatabaseClient.Object,
            _mockEmailClient.Object,
            _validator);
        
        // Act
        var response = orchestrator.RunOrchestrator(mockContext.Object, _mockLogger.Object);
        
        // Assert
        response.ShouldNotBeNull();
        response.Result.Count.ShouldBe(1);
        response.Result.ShouldContain(Constants.ErrorMessages.requestInvalid);
        mockContext.Verify(x => x.CallActivityAsync<bool>(Constants.ActivityNames.validator, It.IsAny<CustomerModel>()), Times.Once);
        mockContext.Verify(x => x.CallActivityAsync<bool>(Constants.ActivityNames.createCustomerThirdParty, It.IsAny<CustomerModel>()), Times.Never);
        mockContext.Verify(x => x.CallActivityAsync<bool>(Constants.ActivityNames.createCustomerCosmosDb, It.IsAny<CustomerModel>()), Times.Never);
        mockContext.Verify(x => x.CallActivityAsync<bool>(Constants.ActivityNames.sendEmailCustomer, It.IsAny<CustomerModel>()), Times.Never);
    }
    
    [Fact]
    public void RunOrchestrator_ActivityCreateCustomerThirdPartyReturnsFalse_ExitsAfter()
    {
        // Arrange
        var mockContext = new Mock<IDurableOrchestrationContext>();

        mockContext.Setup(x => x.CallActivityAsync<bool>(Constants.ActivityNames.validator, It.IsAny<CustomerModel>()))
            .ReturnsAsync(true);
        
        mockContext.Setup(x => x.CallActivityAsync<bool>(Constants.ActivityNames.createCustomerThirdParty, It.IsAny<CustomerModel>()))
            .ReturnsAsync(false);
        
        mockContext.Setup(x => x.CallActivityAsync<bool>(Constants.ActivityNames.createCustomerCosmosDb, It.IsAny<CustomerModel>()))
            .ReturnsAsync(true);
        
        mockContext.Setup(x => x.CallActivityAsync<bool>(Constants.ActivityNames.sendEmailCustomer, It.IsAny<CustomerModel>()))
            .ReturnsAsync(true);
        
        var orchestrator = new Orchestrator(_mockCustomerCreationClient.Object,
            _mockCustomerDatabaseClient.Object,
            _mockEmailClient.Object,
            _validator);
        
        // Act
        var response = orchestrator.RunOrchestrator(mockContext.Object, _mockLogger.Object);
        
        // Assert
        response.ShouldNotBeNull();
        response.Result.Count.ShouldBe(1);
        response.Result.ShouldContain(Constants.ErrorMessages.unableToCreateCustomerThirdParty);
        mockContext.Verify(x => x.CallActivityAsync<bool>(Constants.ActivityNames.validator, It.IsAny<CustomerModel>()), Times.Once);
        mockContext.Verify(x => x.CallActivityAsync<bool>(Constants.ActivityNames.createCustomerThirdParty, It.IsAny<CustomerModel>()), Times.Once);
        mockContext.Verify(x => x.CallActivityAsync<bool>(Constants.ActivityNames.createCustomerCosmosDb, It.IsAny<CustomerModel>()), Times.Never);
        mockContext.Verify(x => x.CallActivityAsync<bool>(Constants.ActivityNames.sendEmailCustomer, It.IsAny<CustomerModel>()), Times.Never);
    }
    
    [Fact]
    public void RunOrchestrator_ActivityCreateCustomerCosmosDbReturnsFalse_ExitsAfter()
    {
        // Arrange
        var mockContext = new Mock<IDurableOrchestrationContext>();

        mockContext.Setup(x => x.CallActivityAsync<bool>(Constants.ActivityNames.validator, It.IsAny<CustomerModel>()))
            .ReturnsAsync(true);
        
        mockContext.Setup(x => x.CallActivityAsync<bool>(Constants.ActivityNames.createCustomerThirdParty, It.IsAny<CustomerModel>()))
            .ReturnsAsync(true);
        
        mockContext.Setup(x => x.CallActivityAsync<bool>(Constants.ActivityNames.createCustomerCosmosDb, It.IsAny<CustomerModel>()))
            .ReturnsAsync(false);
        
        mockContext.Setup(x => x.CallActivityAsync<bool>(Constants.ActivityNames.sendEmailCustomer, It.IsAny<CustomerModel>()))
            .ReturnsAsync(true);
        
        var orchestrator = new Orchestrator(_mockCustomerCreationClient.Object,
            _mockCustomerDatabaseClient.Object,
            _mockEmailClient.Object,
            _validator);
        
        // Act
        var response = orchestrator.RunOrchestrator(mockContext.Object, _mockLogger.Object);
        
        // Assert
        response.ShouldNotBeNull();
        response.Result.Count.ShouldBe(1);
        response.Result.ShouldContain(Constants.ErrorMessages.cosmosDbCustomerCreation);
        mockContext.Verify(x => x.CallActivityAsync<bool>(Constants.ActivityNames.validator, It.IsAny<CustomerModel>()), Times.Once);
        mockContext.Verify(x => x.CallActivityAsync<bool>(Constants.ActivityNames.createCustomerThirdParty, It.IsAny<CustomerModel>()), Times.Once);
        mockContext.Verify(x => x.CallActivityAsync<bool>(Constants.ActivityNames.createCustomerCosmosDb, It.IsAny<CustomerModel>()), Times.Once);
        mockContext.Verify(x => x.CallActivityAsync<bool>(Constants.ActivityNames.sendEmailCustomer, It.IsAny<CustomerModel>()), Times.Never);
    }
    
    [Fact]
    public void RunOrchestrator_ActivitySendEmailCustomerReturnsFalse_ExitsAfter()
    {
        // Arrange
        var mockContext = new Mock<IDurableOrchestrationContext>();

        mockContext.Setup(x => x.CallActivityAsync<bool>(Constants.ActivityNames.validator, It.IsAny<CustomerModel>()))
            .ReturnsAsync(true);
        
        mockContext.Setup(x => x.CallActivityAsync<bool>(Constants.ActivityNames.createCustomerThirdParty, It.IsAny<CustomerModel>()))
            .ReturnsAsync(true);
        
        mockContext.Setup(x => x.CallActivityAsync<bool>(Constants.ActivityNames.createCustomerCosmosDb, It.IsAny<CustomerModel>()))
            .ReturnsAsync(true);
        
        mockContext.Setup(x => x.CallActivityAsync<bool>(Constants.ActivityNames.sendEmailCustomer, It.IsAny<CustomerModel>()))
            .ReturnsAsync(false);
        
        var orchestrator = new Orchestrator(_mockCustomerCreationClient.Object,
            _mockCustomerDatabaseClient.Object,
            _mockEmailClient.Object,
            _validator);
        
        // Act
        var response = orchestrator.RunOrchestrator(mockContext.Object, _mockLogger.Object);
        
        // Assert
        response.ShouldNotBeNull();
        response.Result.Count.ShouldBe(1);
        response.Result.ShouldContain(Constants.ErrorMessages.unableToSendEmail);
        mockContext.Verify(x => x.CallActivityAsync<bool>(Constants.ActivityNames.validator, It.IsAny<CustomerModel>()), Times.Once);
        mockContext.Verify(x => x.CallActivityAsync<bool>(Constants.ActivityNames.createCustomerThirdParty, It.IsAny<CustomerModel>()), Times.Once);
        mockContext.Verify(x => x.CallActivityAsync<bool>(Constants.ActivityNames.createCustomerCosmosDb, It.IsAny<CustomerModel>()), Times.Once);
        mockContext.Verify(x => x.CallActivityAsync<bool>(Constants.ActivityNames.sendEmailCustomer, It.IsAny<CustomerModel>()), Times.Once);
    }
    
    [Fact]
    public void RunOrchestrator_UnhandledExceptionErrorOccurs_ExitsAfter()
    {
        throw new NotImplementedException();
    }

    


}