using Microsoft.Extensions.Configuration;

namespace TinyToolBox.Configuration.Providers;

public sealed class ConfigurationProviderOptions
{
    internal ConfigurationFilter Filter { get; } = new();

    internal ConfigurationFormatter Formatter { get; } = new();

    public ConfigurationProviderOptions AddFormatter<T>(Func<T, string> formatter)
        where T : IConfigurationProvider
    {
        Formatter.Add(formatter);
        return this;
    }
}
