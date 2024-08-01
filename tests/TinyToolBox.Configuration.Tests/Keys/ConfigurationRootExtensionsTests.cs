using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.CommandLine;
using Microsoft.Extensions.Configuration.Memory;
using TinyToolBox.Configuration.Keys;

namespace TinyToolBox.Configuration.Tests.Keys;

public sealed class ConfigurationRootExtensionsTests
{
    private readonly IConfigurationRoot _configurationRoot;

    public ConfigurationRootExtensionsTests()
    {
        var chained = new ConfigurationBuilder()
            .AddConfiguration(
                new ConfigurationBuilder()
                    .AddInMemoryCollection(
                        new[]
                        {
                            new KeyValuePair<string, string?>("4:1", "4-1"),
                            new KeyValuePair<string, string?>("3", "3-3")
                        })
                    .Build())
            .Build();

        _configurationRoot = new ConfigurationBuilder()
            .AddInMemoryCollection(
                new[]
                {
                    new KeyValuePair<string, string?>("1", "1"),
                    new KeyValuePair<string, string?>("2", "2-1"),
                    new KeyValuePair<string, string?>("3", "3-1")
                })
            .AddCommandLine(new[]
            {
                "2=2-2"
            })
            .AddJsonStream(StreamExtensions.Text(@"{ ""3"" : ""3-2"" }"))
            .AddConfiguration(chained)
            .Build();
    }

    public static TheoryData<string, Type> GetProviderCases()
    {
        var theoryData = new TheoryData<string, Type>
        {
            { "1", typeof(MemoryConfigurationProvider) },
            { "2", typeof(CommandLineConfigurationProvider) },
            { "3", typeof(ChainedConfigurationProvider) },
            { "4:1", typeof(ChainedConfigurationProvider) }
        };

        return theoryData;
    }

    [Theory]
    [MemberData(nameof(GetProviderCases))]
    public void GetProvider(string key, Type providerType)
    {
        var provider = _configurationRoot.GetProvider(key);
        provider.Should().BeOfType(providerType);
    }

    [Fact]
    public void AsDictionary()
    {
        var dictionary = _configurationRoot.AsDictionary();
        var pairs = dictionary.ToArray();
        pairs.Should().HaveCount(4);

        pairs.Should().ContainSingle(x => x.Key == "1")
            .Which.Value.Should().BeOfType(typeof(MemoryConfigurationProvider));
        pairs.Should().ContainSingle(x => x.Key == "2")
            .Which.Value.Should().BeOfType(typeof(CommandLineConfigurationProvider));
        pairs.Should().ContainSingle(x => x.Key == "3")
            .Which.Value.Should().BeOfType(typeof(ChainedConfigurationProvider));
        pairs.Should().ContainSingle(x => x.Key == "4:1")
            .Which.Value.Should().BeOfType(typeof(ChainedConfigurationProvider));
    }
}