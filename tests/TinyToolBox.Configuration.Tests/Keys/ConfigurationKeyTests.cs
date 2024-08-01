using FluentAssertions;
using TinyToolBox.Configuration.Keys;

namespace TinyToolBox.Configuration.Tests.Keys;

public sealed class ConfigurationKeyTests
{
    [Theory]
    [InlineData("Test", "Test")]
    [InlineData(":Test", "Test")]
    public void Path(string path, string expected)
    {
        var key = new ConfigurationKey(path);
        key.Path.Should().Be(expected);

        key = (ConfigurationKey)path;
        key.Path.Should().Be(expected);
    }

    [Theory]
    [InlineData("Test", "Test")]
    [InlineData("Test:key", "key")]
    [InlineData("Test:path:key", "key")]
    public void GetKey(string path, string expected)
    {
        var key = new ConfigurationKey(path);
        key.GetKey().Should().Be(expected);

        key = (ConfigurationKey)path;
        key.GetKey().Should().Be(expected);
    }

    [Fact]
    public void GetSegments()
    {
        var key = ConfigurationKey.Empty;
        var segments = key.GetSegments();
        segments.Should().BeEquivalentTo("");

        key = new ConfigurationKey("1");
        segments = key.GetSegments();
        segments.Should().BeEquivalentTo("1");

        key = new ConfigurationKey("1:2:3");
        segments = key.GetSegments();
        segments.Should().BeEquivalentTo("1", "2", "3");
    }

    [Theory]
    [InlineData("Test", "", "Test")]
    [InlineData("1:2:3", "", "1")]
    [InlineData("1:2:3", "2", null)]
    [InlineData("1:2:3", "1:3", null)]
    [InlineData("1:2:3", "1", "1:2")]
    [InlineData("1:2:3", "1:2", "1:2:3")]
    public void ChildOf(string path, string parent, string? expected)
    {
        var key = new ConfigurationKey(path);

        var child = key.ChildOf((ConfigurationKey)parent);
        if (expected is null)
            child.Should().BeNull();
        else
            child!.Value.Should().Be((ConfigurationKey)expected);
    }

    [Theory]
    [InlineData("", 1)]
    [InlineData(":", 1)]
    [InlineData(":1", 1)]
    [InlineData("1", 1)]
    [InlineData("1:2", 2)]
    [InlineData("1:2:3", 3)]
    public void Depth(string path, int expected)
    {
        var key = new ConfigurationKey(path);
        key.Depth().Should().Be(expected);

        key = (ConfigurationKey)path;
        key.Depth().Should().Be(expected);
    }

    [Theory]
    [InlineData("Test", "test", true)]
    [InlineData("test", "TEST", true)]
    [InlineData("Test", "tes", false)]
    [InlineData("Test", "", false)]
    public void Equality(string path1, string path2, bool expected)
    {
        var key1 = new ConfigurationKey(path1);
        var key2 = new ConfigurationKey(path2);

        var actual = key1.Equals(key2);
        actual.Should().Be(expected);

        actual = key1 == key2;
        actual.Should().Be(expected);

        key1 = (ConfigurationKey)path1;
        key2 = (ConfigurationKey)path2;

        actual = key1.Equals(key2);
        actual.Should().Be(expected);

        actual = key1 == key2;
        actual.Should().Be(expected);
    }
}