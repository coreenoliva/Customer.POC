using System;
using Customer.POC.Settings;
using Microsoft.Extensions.Options;

namespace Test.TestData;

public static class OptionsSettingsTestData
{
    public static IOptions<Settings> DefaultSettings => 
        Options.Create<Settings>(
            new Settings()
            {
                CustomerCreationApiBaseUrl = new Uri("https://baseUrl")
                
            });
}