using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Customer.POC.Clients.Abstractions;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Customer.POC.Models;
using Customer.POC.Validators;
using Microsoft.AspNetCore.Hosting.Internal;
using Microsoft.AspNetCore.Mvc;

namespace Customer.POC;

public class Orchestrator
{
    
    private readonly ICustomerCreationClient _customerCreationClient;
    private readonly ICustomerDatabaseClient _customerDatabaseClient;
    private readonly IEmailClient _emailClient;

    public Orchestrator(
        ICustomerCreationClient customerCreationClient, 
        ICustomerDatabaseClient customerDatabaseClient,
        IEmailClient emailClient)
    {
        _customerCreationClient = customerCreationClient;
        _customerDatabaseClient = customerDatabaseClient;
        _emailClient = emailClient;
    }
    
    [FunctionName("Orchestrator")]
    public async Task<List<string>> RunOrchestrator(
        [OrchestrationTrigger] IDurableOrchestrationContext context)
    {
        var outputs = new List<string>();
        try
        {
            var request = context.GetInput<CustomerModel>();
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
                    }
                }
            }
            else
            {
                // invalid requests
                outputs.Add("Request invalid");
            }
        }
        catch (Exception ex)
        {
            outputs.Add("Unexpected error occurred.");
        }

        return outputs;
    }

    [FunctionName("Activity_CreateCustomer_CosmosDb")]
    public async Task<bool>CreateCustomerCosmosDb([ActivityTrigger] CustomerModel customerRequest, ILogger log)
    {
        log.LogInformation("Creating customer in cosmos db");
        var customerDbCreationResult = await _customerDatabaseClient.CreateCustomer(customerRequest);
        if (customerDbCreationResult)
        {
            // log id
            log.LogInformation("Customer successfully created in cosmos db");
            return true;
        }
        else
        {
            log.LogInformation("Customer not updated in cosmos db");
        }

        return false;
    }
    [FunctionName("Activity_CreateCustomer_ThirdParty")]
    public async Task<bool> CreateCustomerThirdParty([ActivityTrigger] CustomerModel customerRequest, ILogger log)
    {
        log.LogInformation("Creating Customer in third party API");
        var customerCreationResult = await _customerCreationClient.CreateCustomerAsync(customerRequest);

        if (customerCreationResult)
        {
            log.LogInformation(("Yay customer created"));
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
        log.LogInformation($"Validating Request Payload");
        var validator = new InputValidator();
        
        // todo update to pass on error message from validator
        log.LogInformation("Validating request");
        
        var validationResult = validator.Validate(customerRequest);

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
        string instanceId = await starter.StartNewAsync("Orchestrator", customerRequest);

        log.LogInformation($"Started orchestration with ID = '{instanceId}'.");

        return starter.CreateCheckStatusResponse(req, instanceId);
    }
}