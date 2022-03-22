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

        // builder.Services.AddSingleton<Settings.Settings>();
        builder.Services.AddSingleton<AbstractValidator<CustomerModel>, InputValidator>();
        // builder.Services.Add(ServiceDescriptor.Singleton(typeof(Settings.Settings), builder.GetCustomConfiguration()));
        //
        var serviceProvider = builder.Services.BuildServiceProvider();
        var existingConfig = serviceProvider.GetService<IConfiguration>();
        
        var configBuilder = new ConfigurationBuilder()
            .AddConfiguration(existingConfig)
            .AddEnvironmentVariables()
            .Build();
        
        
        var connectionString = configBuilder["AppConfigurationConnectionString"];
        // builder.Services.AddOptions<Settings.Settings>()
        //     .Configure<IConfiguration>((settings, configuration) =>
        //     {
        //         configuration.GetSection("Values").Bind(settings);
        //     });
        builder.Services.AddOptions<Settings.Settings>()
            .Configure<IConfiguration>((settings, configuration) =>
            {
                configuration.Bind(settings);
            });
        
        
        // builder.Services.AddSingleton<IGitHubApiService, GitHubApiService>();
        builder.Services.AddSingleton<ICustomerCreationClient, CustomerCreationClient>();
                // var serviceProvider = builder.Services.BuildServiceProvider();
                // var existingConfig = serviceProvider.GetService<IConfiguration>();
                //
                // var configBuilder = new ConfigurationBuilder()
                //     .AddConfiguration(existingConfig)
                //     .AddEnvironmentVariables();
                //
                // var builtConfig = configBuilder.Build();
                // var appConfigConnectionString = builtConfig["AppConfigurationConnectionString"];



        
        


    }

}

public static class ServiceExtensions
{
    // public static Settings.Settings GetCustomConfiguration(this IFunctionsHostBuilder builder)
    // {
    //     var serviceProvider = builder.Services.BuildServiceProvider();
    //     var existingConfig = serviceProvider.GetService<IConfiguration>();
    //     var settings = new Settings.Settings(existingConfig);
    //     builder.Services.AddSingleton<ISettings, Settings.Settings>();
    //     return settings;
    //     
    // }
}