using System;

namespace Customer.POC.Settings;

public class Settings
{
    public Uri CustomerCreationApiBaseUrl { get; set; }
    public Uri CosmosDbConnectionString { get; set; }
    public string CosmosAccessKey { get; set; }
    public string SendGridKey { get; set; }
}