using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Customer.POC.Clients.Abstractions;
using Customer.POC.Models;
using Customer.POC.Validators;
using FluentValidation;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Customer.POC;

public class Orchestrator
{
    private readonly ICustomerCreationClient _customerCreationClient;
    private readonly ICustomerDatabaseClient _customerDatabaseClient;
    private readonly IEmailClient _emailClient;
    private readonly AbstractValidator<CustomerModel> _inputValidator;

    public Orchestrator(
        ICustomerCreationClient customerCreationClient,
        ICustomerDatabaseClient customerDatabaseClient,
        IEmailClient emailClient,
        AbstractValidator<CustomerModel> inputValidator)
    {
        _customerCreationClient = customerCreationClient;
        _customerDatabaseClient = customerDatabaseClient;
        _emailClient = emailClient;
        _inputValidator = inputValidator;
    }

    [FunctionName("Orchestrator")]
    public async Task<List<string>> RunOrchestrator(
        [OrchestrationTrigger] IDurableOrchestrationContext context, ILogger log)
    {
        var outputs = new List<string>();
        try
        {
            var request = context.GetInput<CustomerModel>();

            // todo add retries for transient errors 
            // validate 
            var validationResult =
                await context.CallActivityAsync<bool>("Activity_Validator", request);

            if (validationResult)
            {
                // create customer by calling 3rd party
                var customerCreationResult =
                    await context.CallActivityAsync<bool>("Activity_CreateCustomer_ThirdParty", request);
                if (customerCreationResult)
                {
                    // retrieve something from table storage- what about cacheing the data?
                    // create customer in database (cosmos db) 
                    // todo use managed identity 
                    var customerCosmosCreationResult =
                        await context.CallActivityAsync<bool>("Activity_CreateCustomer_CosmosDb", request);

                    if (customerCosmosCreationResult)
                    {
                        // send email
                        var sendEmailResult =
                            await context.CallActivityAsync<bool>("Activity_SendEmail_Customer", request);
                        if (!sendEmailResult) outputs.Add("Error sending customer created email");
                    }
                    else
                    {
                        {
                            outputs.Add("Unable to create customer in cosmos db");
                        }
                    }
                }
                else
                {
                    outputs.Add("Unable to create customer in third party API");
                }
            }
            else
            {
                outputs.Add("Request invalid");
            }
        }
        catch (Exception ex)
        {
            outputs.Add("Unhandled exception while creating customer");
            log.LogError(ex, "Unhandled exception while creating customer");
            // throw error to fail the orchestration
            throw;
        }

        return outputs;
    }

    [FunctionName("Activity_CreateCustomer_CosmosDb")]
    public async Task<bool> CreateCustomerCosmosDb([ActivityTrigger] CustomerModel customerRequest, ILogger log)
    {
        log.LogInformation("Creating customer in cosmos db");
        var customerDbCreationResult = await _customerDatabaseClient.CreateCustomer(customerRequest);
        if (customerDbCreationResult != null)
        {
            log.LogInformation($"Customer successfully created in cosmos db. Id: {customerDbCreationResult}");
            return true;
        }

        log.LogInformation("Customer not updated in cosmos db");
        return false;
    }

    [FunctionName("Activity_CreateCustomer_ThirdParty")]
    public async Task<bool> CreateCustomerThirdParty([ActivityTrigger] CustomerModel customerRequest, ILogger log)
    {
        log.LogInformation("Creating Customer in third party API");
        var customerCreationResult = await _customerCreationClient.CreateCustomerAsync(customerRequest);

        if (customerCreationResult)
        {
            log.LogInformation("Yay customer created");
            return true;
        }

        return false;
    }

    [FunctionName("Activity_SendEmail_Customer")]
    public async Task<bool> SendCustomerEmail([ActivityTrigger] CustomerModel customerRequest, ILogger log)
    {
        log.LogInformation("Sending email");
        var sendEmailResult = await _emailClient.SendCustomerCreatedEmail(customerRequest);
        if (sendEmailResult)
        {
            log.LogInformation("Email successfully sent");
            return true;
        }

        log.LogInformation("Unable to send email");
        return false;
    }

    [FunctionName("Activity_Validator")]
    public bool ValidateRequest([ActivityTrigger] CustomerModel customerRequest, ILogger log)
    {
        log.LogInformation("Validating Request Payload");
        
        // todo update to pass on error message from validator
        log.LogInformation("Validating request");

        var validationResult = _inputValidator.Validate(customerRequest);

        return validationResult.IsValid;
    }


    [FunctionName("Orchestrator_HttpStart")]
    public async Task<HttpResponseMessage> HttpStart(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post")]
        HttpRequestMessage req,
        [DurableClient] IDurableOrchestrationClient starter,
        ILogger log)
    {
        var customerRequest = JsonConvert.DeserializeObject<CustomerModel>(req.Content.ReadAsStringAsync().Result);

        // Function input comes from the request content.
        var instanceId = await starter.StartNewAsync("Orchestrator", customerRequest);

        log.LogInformation($"Started orchestration with ID = '{instanceId}'.");

        return starter.CreateCheckStatusResponse(req, instanceId);
    }
}