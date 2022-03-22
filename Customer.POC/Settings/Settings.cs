using System;
using Microsoft.Extensions.Configuration;

namespace Customer.POC.Settings;


// private IConfiguration Configuration { get; set; }

public class Settings
{

    public string CustomerCreationApiBaseUrl { get; set; }
}
    // public Settings(IConfiguration configuration)
    // {
    //     Configuration = configuration;
    // }
    // public string CustomerCreationApiBaseUrl => GetSetting<string>("CustomerCreationAPIBaseUrl");
    //
    // private T GetSetting<T>(string key) where T : IConvertible
    // {
    //     return Configuration.GetValue<T>(key);
    // }