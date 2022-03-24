using System.IO;
using Azure.Data.AppConfiguration;
using Customer.POC.Clients;
using Customer.POC.Clients.Abstractions;
using Customer.POC.Models;
using Customer.POC.Settings;
using Customer.POC.Validators;
using DurableTask.Core.Settings;
using FluentValidation;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;

using Microsoft.Extensions.DependencyInjection;

[assembly: FunctionsStartup(typeof(Customer.POC.Startup))]
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
        
        // var connectionString = configBuilder["AppConfigurationConnectionString"];
        builder.Services.AddOptions<Settings.Settings>()
            .Configure<IConfiguration>((settings, configuration) =>
            {
                configuration.Bind(settings);
            });
        
        builder.Services.AddSingleton<ICustomerCreationClient, CustomerCreationClient>();
        builder.Services.AddSingleton<ICustomerDatabaseClient, CustomerDatabaseClient>();
        builder.Services.AddSingleton<IEmailClient, EmailClient>();
    }
}