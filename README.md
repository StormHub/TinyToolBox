
# TinyToolBox Configuration

[![Nuget](https://img.shields.io/nuget/v/TinyToolBox.Configuration?label=TinyToolBox.Configuration)](https://www.nuget.org/packages/TinyToolBox.Configuration/)
[![Nuget](https://img.shields.io/nuget/v/TinyToolBox.Configuration.AspNetCore?label=TinyToolBox.Configuration.AspNetCore)](https://www.nuget.org/packages/TinyToolBox.Configuration.AspNetCore/)

Super simple application configuration source inspection utility, especially when applications have lots of configuration providers and frequently overriden at deployment time.
It should also be noted that it is ONLY meant to provide where are the configuration values coming from, NEVER intended to provides the actual values! 

* Support configuration navigation path
* Alternative flat style

## Example api results
Start [example AspNetCore application](https://github.com/StormHub/TinyToolBox/tree/main/examples/Configuration/WebApi.Minimal) on development mode

*Navigation to http://localhost:5000/api/configuration for all configuration values should result in the following response
```yaml
{
    "applicationName": "MemoryConfigurationProvider (Chained:1)",
    "contentRoot": "MemoryConfigurationProvider",
    "hostingStartupAssemblies": "MemoryConfigurationProvider (Chained:1)",
    "Logging": {
        "LogLevel": {
            "Default": "appsettings.Development.json (Optional)",
            "Microsoft.AspNetCore": "appsettings.Development.json (Optional)"
        }
    },
    "WeatherForecast": {
        "Activities": {
            "Friday": "appsettings.json (Optional)",
            "Monday": "appsettings.json (Optional)",
            "Saturday": "appsettings.json (Optional)",
            "Sunday": "appsettings.json (Optional)",
            "Thursday": "appsettings.json (Optional)",
            "Tuesday": "appsettings.json (Optional)",
            "Wednesday": "appsettings.json (Optional)"
        },
        "Summaries": {
            "0": "appsettings.json (Optional)",
            "1": "appsettings.json (Optional)",
            "2": "appsettings.json (Optional)",
            "3": "appsettings.json (Optional)",
            "4": "appsettings.json (Optional)",
            "5": "appsettings.json (Optional)",
            "6": "appsettings.json (Optional)",
            "7": "appsettings.json (Optional)",
            "8": "appsettings.json (Optional)",
            "9": "appsettings.json (Optional)"
        }
    }
}
```

*Navigation to a specific configuration node http://localhost:5000/api/configuration/Logging
```yaml
{
    "logging": {
        "LogLevel": {
            "Default": "appsettings.Development.json (Optional)",
            "Microsoft.AspNetCore": "appsettings.Development.json (Optional)"
        }
    }
}
```

*Alternative flat style http://localhost:5000/api/configuration/Logging?style=flat
```yaml
{
    "logging:LogLevel:Default": "appsettings.Development.json (Optional)",
    "logging:LogLevel:Microsoft.AspNetCore": "appsettings.Development.json (Optional)"
}
```

## Setup
```C#
// IServiceCollection services
services.AddConfigurationProviderOptions(options =>
{
    // Map configuration provider for names if needed
    options.Map<EnvironmentVariablesConfigurationProvider>("Environment Variable");
});
```
This adds the custom configuration provider information, not needed for default options. 


```C#
// IApplicationBuilder app
// Map api endpoint, Default to /api/configuration/{*path}
app.MapConfigurationEndpoint(); 
```
Note the catch all path for configuration path navigations