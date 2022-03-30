using Customer.POC;
using Customer.POC.Clients;
using Customer.POC.Clients.Abstractions;
using Customer.POC.Models;
using Customer.POC.Validators;
using FluentValidation;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SendGrid.Extensions.DependencyInjection;

[assembly: FunctionsStartup(typeof(Startup))]

namespace Customer.POC;

public class Startup : FunctionsStartup
{
    public override void Configure(IFunctionsHostBuilder builder)
    {
        builder.Services.AddSingleton<AbstractValidator<CustomerModel>, InputValidator>();
        var serviceProvider = builder.Services.BuildServiceProvider();
        var existingConfig = serviceProvider.GetService<IConfiguration>();
        
        var configBuilder = new ConfigurationBuilder()
            .AddConfiguration(existingConfig)
            .AddEnvironmentVariables()
            .Build();

        builder.Services.AddOptions<Settings.Settings>()
            .Configure<IConfiguration>((settings, configuration) => { configuration.Bind(settings); });

        
        builder.Services.AddSendGrid(x => x.ApiKey = configBuilder["SendGridKey"]);
        builder.Services.AddSingleton(x => new CosmosClient(configBuilder[$"CosmosDbConnectionString"]));
        builder.Services.AddHttpClient<ICustomerCreationClient, CustomerCreationClient>();
        builder.Services.AddSingleton<ICustomerDatabaseClient, CustomerDatabaseClient>();
        builder.Services.AddSingleton<IEmailClient, EmailClient>();
    }
}