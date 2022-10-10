using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Memory;
using TinyToolBox.Configuration.Keys;
using FluentAssertions;

namespace TinyToolBox.Configuration.Tests.Keys;

public sealed class ConfigurationDictionaryTests
{
    private readonly ConfigurationDictionary _keyDictionary;

    public ConfigurationDictionaryTests()
    {
        var configurationRoot = new ConfigurationBuilder()
            .AddInMemoryCollection(
                new[]
                {
                    new KeyValuePair<string, string?>("1:2:1", "1-2-1"),
                    new KeyValuePair<string, string?>("1:2", "1-2"),
                    new KeyValuePair<string, string?>("1:2:3", "1-2-3"),
                    new KeyValuePair<string, string?>("2", null),
                })
            .Build();
        _keyDictionary = configurationRoot.AsDictionary();
    }

    public static TheoryData<string, string[]> GetChildKeysCases()
    {
        var theoryData = new TheoryData<string, string[]>
        {
            { "", new[] { "1", "2" } },
            { "1", new[] { "1:2" } },
            { "2", Array.Empty<string>() },
            { "3", Array.Empty<string>() },
            { "1:2", new[] { "1:2:1", "1:2:3" } },
            { "1:2:1", Array.Empty<string>() },
            { "1:2:2", Array.Empty<string>() }
        };

        return theoryData;
    }

    [Theory]
    [MemberData(nameof(GetChildKeysCases))]
    public void GetChildKeys(string parent, string[] expected)
    {
        var childKeys = _keyDictionary.GetChildKeys(parent);
        childKeys.Should().BeEquivalentTo(expected.Select(x => new ConfigurationKey(x)).ToArray());
    }

    public static TheoryData<string, bool?> TryGetValueCases()
    {
        var theoryData = new TheoryData<string, bool?>
        {
            { "" , false },
            { "1" , false },
            { "2", true },
            { "3", false },
            { "1:2", true },
        };

        return theoryData;
    }

    [Theory]
    [MemberData(nameof(TryGetValueCases))]
    public void TryGetValue(string key, bool exists)
    {
        _keyDictionary.TryGetValue(key, out var actual).Should().Be(exists);
        if (exists)
        {
            actual.Should().BeOfType<MemoryConfigurationProvider>();
        }
    }
}
