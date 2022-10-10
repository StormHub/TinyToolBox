using Microsoft.Extensions.Configuration;
using TinyToolBox.Configuration.Extensions;

namespace TinyToolBox.Configuration.Tests.Extensions;

[UsesVerify]
public sealed class JsonResultTests : IDisposable
{
    private readonly IConfigurationRoot _configurationRoot;
    private readonly MemoryStream _memoryStream;

    public JsonResultTests() 
    {
        var builder = new ConfigurationBuilder()
            .AddInMemoryCollection(new[]
                {
                    new KeyValuePair<string, string?>("Logging:LogLevel:Microsoft.AspNetCore", "Debug"),
                    new KeyValuePair<string, string?>("HostingOptions:Timeout", "10"),
                    new KeyValuePair<string, string?>("HostingOptions:Url", "http://development"),
                })
            .AddJsonStream(StreamExtensions.Text(
                @"{
                    ""Logging"" : { 
                        ""LogLevel"" : { 
                            ""Default"" : ""Information"" ,
                            ""Microsoft.AspNetCore"" : ""Warning"",
                            ""Microsoft.Hosting.Lifetime"" : ""Information""
                        } 
                    },
                    ""HostingOptions"" : { 
                       ""Timeout"" : 20 
                    }
                }"))
            .AddCommandLine(new[]
                {
                    $"Logging:LogLevel:Microsoft.AspNetCore=Error",
                    $"HostingOptions:Url=http://production",
                });

        _configurationRoot = builder.Build();
        _memoryStream = new MemoryStream();
    }

    [Fact]
    public async Task AsJsonNode()
    {
        var jsonNode = _configurationRoot.AsJsonNode();
        await jsonNode.WriteToStream(_memoryStream);

        var content = await _memoryStream.ReadAsString();
        await VerifyJson(content);
    }

    [Fact]
    public async Task AsDictionary()
    {
        var dictionary = _configurationRoot.AsDictionary();
        await dictionary.WriteToStream(_memoryStream);

        var content = await _memoryStream.ReadAsString();
        await VerifyJson(content);
    }

    [Fact]
    public async Task AsJsonNodeWithPath()
    {
        var jsonNode = _configurationRoot.AsJsonNode(new[] { "Logging", "LogLevel" });
        await jsonNode.WriteToStream(_memoryStream);

        var content = await _memoryStream.ReadAsString();
        await VerifyJson(content);
    }

    [Fact]
    public async Task AsDictionaryWithPath()
    {
        var dictionary = _configurationRoot.AsDictionary(new[] { "Logging", "LogLevel" });
        await dictionary.WriteToStream(_memoryStream);

        var content = await _memoryStream.ReadAsString();
        await VerifyJson(content);
    }

    [Fact]
    public async Task AsJsonNodeEmpty()
    {
        var jsonNode = _configurationRoot.AsJsonNode(new[] { "invalid" });
        await jsonNode.WriteToStream(_memoryStream);

        var content = await _memoryStream.ReadAsString();
        await VerifyJson(content);
    }

    [Fact]
    public async Task AsDictionaryEmpty()
    {
        var dictionary = _configurationRoot.AsDictionary(new[] { "invalid" });
        await dictionary.WriteToStream(_memoryStream);

        var content = await _memoryStream.ReadAsString();
        await VerifyJson(content);
    }

    public void Dispose() => _memoryStream.Dispose();
}
