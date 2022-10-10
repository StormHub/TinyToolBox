using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace TinyToolBox.Configuration.Tests;

internal static class StreamExtensions
{
    public static Stream Text(string value)
    {
        var memStream = new MemoryStream();
        var textWriter = new StreamWriter(memStream);
        textWriter.Write(value);
        textWriter.Flush();
        memStream.Seek(0, SeekOrigin.Begin);

        return memStream;
    }

    public static string JsonFilePath(
        string? folder = null,
        string? key = null,
        [CallerFilePath] string testFilePath = "")
    {
        var directoryName = Path.GetDirectoryName(testFilePath);
        Debug.Assert(directoryName != null);

        var fileName = Path.GetFileNameWithoutExtension(testFilePath);
        var name = $"{fileName}";
        if (key != null)
        {
            name += $"-{key}";
        }

        var paths = new List<string>
        {
            directoryName
        };
        if (!string.IsNullOrEmpty(folder))
        {
            paths.Add(folder);
        }
        paths.Add($"{name}.json");

        var fullPath = Path.Combine(paths.ToArray());
        return fullPath;
    }

    public static async Task<string> ReadAsString(this Stream stream)
    {
        stream.Seek(0, SeekOrigin.Begin);
        using var reader = new StreamReader(stream);
        var content = await reader.ReadToEndAsync();

        return content;
    }

    public static async Task WriteToStream(this IReadOnlyDictionary<string, string> dictionary, Stream stream)
    {
        await JsonSerializer.SerializeAsync(stream, dictionary);
    }

    public static async Task WriteToStream(this JsonNode jsonNode, Stream stream)
    {
        await using var writer = new Utf8JsonWriter(stream);
        jsonNode.WriteTo(writer);
        await writer.FlushAsync();
    }
}
