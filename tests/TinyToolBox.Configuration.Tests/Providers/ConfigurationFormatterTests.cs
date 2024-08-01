using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.CommandLine;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.Configuration.Memory;
using TinyToolBox.Configuration.Keys;
using TinyToolBox.Configuration.Providers;

namespace TinyToolBox.Configuration.Tests.Providers;

public sealed class ConfigurationFormatterTests
{
    private readonly IConfigurationRoot _chainedRoot;
    private readonly ConfigurationFormatter _configurationFormatter;
    private readonly IConfigurationRoot _configurationRoot;

    public ConfigurationFormatterTests()
    {
        var path = StreamExtensions.JsonFilePath("files");
        var chainedPath = StreamExtensions.JsonFilePath("files", "Chained");

        _chainedRoot = new ConfigurationBuilder()
            .AddJsonFile(chainedPath, true)
            .Build();

        var chained = new ConfigurationBuilder()
            .AddConfiguration(
                new ConfigurationBuilder()
                    .AddInMemoryCollection(
                        new[]
                        {
                            new KeyValuePair<string, string?>("5:1", "5-1")
                        })
                    .AddConfiguration(_chainedRoot)
                    .Build())
            .Build();

        _configurationRoot = new ConfigurationBuilder()
            .AddInMemoryCollection(
                new[]
                {
                    new KeyValuePair<string, string?>("1", "1")
                })
            .AddCommandLine(new[]
            {
                "2=2"
            })
            .AddJsonStream(StreamExtensions.Text(@"{ ""3"" : ""3"" }"))
            .AddJsonFile(path, false)
            .AddConfiguration(chained)
            .Build();

        _configurationFormatter = new();
    }

    [Fact]
    public void Fallback()
    {
        var commandLineProvider = _configurationRoot.Providers.OfType<CommandLineConfigurationProvider>().Single();

        _configurationFormatter.Format("2", commandLineProvider)
            .Should().Be($"{nameof(CommandLineConfigurationProvider)}");
        _configurationFormatter
            .Format(new KeyValuePair<ConfigurationKey, IConfigurationProvider>("2", commandLineProvider))
            .Should().Be($"{nameof(CommandLineConfigurationProvider)}");
    }

    [Fact]
    public void Files()
    {
        var jsonProvider = _configurationRoot.Providers.OfType<JsonConfigurationProvider>().Single();

        // Default
        _configurationFormatter.Format("4", jsonProvider)
            .Should().Be($"{jsonProvider.Source.Path} (Required)");
        _configurationFormatter.Format(new KeyValuePair<ConfigurationKey, IConfigurationProvider>("4", jsonProvider))
            .Should().Be($"{jsonProvider.Source.Path} (Required)");

        // Override
        _configurationFormatter.Add<JsonConfigurationProvider>(_ => "MyJsonProvider");
        _configurationFormatter.Format("4", jsonProvider)
            .Should().Be("MyJsonProvider");
        _configurationFormatter.Format(new KeyValuePair<ConfigurationKey, IConfigurationProvider>("4", jsonProvider))
            .Should().Be("MyJsonProvider");
    }

    [Fact]
    public void Streams()
    {
        var streamProvider = _configurationRoot.Providers.OfType<JsonStreamConfigurationProvider>().Single();

        // Default
        _configurationFormatter.Format("3", streamProvider)
            .Should().Be($"{streamProvider} ({streamProvider.Source.Stream!.GetType().Name})");
        _configurationFormatter.Format(new KeyValuePair<ConfigurationKey, IConfigurationProvider>("3", streamProvider))
            .Should().Be($"{streamProvider} ({streamProvider.Source.Stream!.GetType().Name})");

        // Override
        _configurationFormatter.Add<JsonStreamConfigurationProvider>(_ => "MyStreamProvider");
        _configurationFormatter.Format("3", streamProvider)
            .Should().Be("MyStreamProvider");
        _configurationFormatter.Format(new KeyValuePair<ConfigurationKey, IConfigurationProvider>("3", streamProvider))
            .Should().Be("MyStreamProvider");
    }

    [Fact]
    public void Chained()
    {
        var chainedProvider = _configurationRoot.Providers.OfType<ChainedConfigurationProvider>().Single();
        var jsonProvider = _chainedRoot.Providers.OfType<JsonConfigurationProvider>().Single();

        // Default
        _configurationFormatter.Format("5:1", chainedProvider)
            .Should().Be($"{nameof(MemoryConfigurationProvider)} (Chained:2)");
        _configurationFormatter
            .Format(new KeyValuePair<ConfigurationKey, IConfigurationProvider>("5:1", chainedProvider))
            .Should().Be($"{nameof(MemoryConfigurationProvider)} (Chained:2)");

        // Chained to default
        _configurationFormatter.Format("6", chainedProvider)
            .Should().Be($"{jsonProvider.Source.Path} (Optional) (Chained:3)");
        _configurationFormatter.Format(new KeyValuePair<ConfigurationKey, IConfigurationProvider>("6", chainedProvider))
            .Should().Be($"{jsonProvider.Source.Path} (Optional) (Chained:3)");

        // Override
        _configurationFormatter.Add<ChainedConfigurationProvider>(_ => "MyChainedProvider");
        _configurationFormatter.Format("5:1", chainedProvider)
            .Should().Be("MyChainedProvider");
        _configurationFormatter
            .Format(new KeyValuePair<ConfigurationKey, IConfigurationProvider>("5:1", chainedProvider))
            .Should().Be("MyChainedProvider");
    }

    [Fact]
    public void OverrideSameType()
    {
        var memoryProvider = _configurationRoot.Providers.OfType<MemoryConfigurationProvider>().Single();

        _configurationFormatter.Add<MemoryConfigurationProvider>(_ => $"1-{nameof(MemoryConfigurationProvider)}");
        _configurationFormatter.Add<MemoryConfigurationProvider>(_ => $"2-{nameof(MemoryConfigurationProvider)}");

        _configurationFormatter.Format("1", memoryProvider)
            .Should().Be($"2-{nameof(MemoryConfigurationProvider)}");
        _configurationFormatter.Format(new KeyValuePair<ConfigurationKey, IConfigurationProvider>("1", memoryProvider))
            .Should().Be($"2-{nameof(MemoryConfigurationProvider)}");
    }
}