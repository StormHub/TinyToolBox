using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.CommandLine;
using Microsoft.Extensions.Configuration.Memory;
using TinyToolBox.Configuration.Keys;
using TinyToolBox.Configuration.Providers;

namespace TinyToolBox.Configuration.Tests.Providers;

public sealed class ConfigurationFilterTests
{
    private readonly IConfigurationRoot _configurationRoot;

    public ConfigurationFilterTests()
    {
        _configurationRoot = new ConfigurationBuilder()
            .AddInMemoryCollection(
                new[]
                {
                    new KeyValuePair<string, string?>("1:2:1", "1-2-1"),
                    new KeyValuePair<string, string?>("1:2", "1-2"),
                    new KeyValuePair<string, string?>("1:2:3", "1-2-3"),
                    new KeyValuePair<string, string?>("2", null)
                })
            .AddCommandLine(new[]
                {
                    $"2:1:1=2-1-1",
                    $"3=3"
                })
            .Build();
    }

    [Fact]
    public void ProviderTypes()
    {
        var memoryProvider = _configurationRoot.Providers.OfType<MemoryConfigurationProvider>().Single();
        var commandLineProvider = _configurationRoot.Providers.OfType<CommandLineConfigurationProvider>().Single();

        var configurationFilter = new ConfigurationFilter(
            new Dictionary<Type, Func<KeyValuePair<ConfigurationKey, IConfigurationProvider>, bool>>
            {
                { typeof(MemoryConfigurationProvider), (pair) => pair.Key.Path.StartsWith("1:2") },
                { typeof(CommandLineConfigurationProvider), (pair) => pair.Key.Path == "3" },
            });
        var result = configurationFilter.Apply(_configurationRoot.Enumerate()).ToArray();

        result.Should().BeEquivalentTo(
            new[]
            {
                new KeyValuePair<ConfigurationKey, IConfigurationProvider>(
                    new ConfigurationKey("1:2:1"), memoryProvider),
                new KeyValuePair<ConfigurationKey, IConfigurationProvider>(
                    new ConfigurationKey("1:2"), memoryProvider),
                new KeyValuePair<ConfigurationKey, IConfigurationProvider>( 
                    new ConfigurationKey("1:2:3"), memoryProvider),
                new KeyValuePair<ConfigurationKey, IConfigurationProvider>(
                    new ConfigurationKey("3"), commandLineProvider)
            });
    }

    [Fact]
    public void Empty()
    {
        var configurationFilter = new ConfigurationFilter(
            new Dictionary<Type, Func<KeyValuePair<ConfigurationKey, IConfigurationProvider>, bool>>());
        var memoryProvider = _configurationRoot.Providers.OfType<MemoryConfigurationProvider>().Single();
        var commandLineProvider = _configurationRoot.Providers.OfType<CommandLineConfigurationProvider>().Single();

        var result = configurationFilter.Apply(_configurationRoot.Enumerate()).ToArray();

        result.Should().BeEquivalentTo(
            new[]
            {
                new KeyValuePair<ConfigurationKey, IConfigurationProvider>(
                    new ConfigurationKey("1:2:1"), memoryProvider),
                new KeyValuePair<ConfigurationKey, IConfigurationProvider>(
                    new ConfigurationKey("1:2"), memoryProvider),
                new KeyValuePair<ConfigurationKey, IConfigurationProvider>(
                    new ConfigurationKey("1:2:3"), memoryProvider),
                new KeyValuePair<ConfigurationKey, IConfigurationProvider>(
                    new ConfigurationKey("2"), memoryProvider),
                new KeyValuePair<ConfigurationKey, IConfigurationProvider>(
                    new ConfigurationKey("2:1:1"), commandLineProvider),
                new KeyValuePair<ConfigurationKey, IConfigurationProvider>(
                    new ConfigurationKey("3"), commandLineProvider)
        });
    }
}
