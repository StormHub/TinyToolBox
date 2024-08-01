using System.Text.Json.Nodes;
using Microsoft.Extensions.Configuration;
using TinyToolBox.Configuration.Keys;
using TinyToolBox.Configuration.Providers;

namespace TinyToolBox.Configuration.Extensions;

public static class ConfigurationExtensions
{
    public static IReadOnlyDictionary<string, string> AsDictionary(
        this IConfigurationRoot configurationRoot,
        string[]? segments = null,
        ConfigurationProviderOptions? options = null)
    {
        options ??= new ConfigurationProviderOptions();
        var configurationDictionary = configurationRoot.AsDictionary(segments, options.Filter);

        return configurationDictionary.ToDictionary(x => x.Key.Path, options.Formatter.Format);
    }

    public static JsonNode AsJsonNode(
        this IConfigurationRoot configurationRoot,
        string[]? segments = null,
        ConfigurationProviderOptions? options = null)
    {
        options ??= new ConfigurationProviderOptions();
        var configurationDictionary = configurationRoot.AsDictionary(segments, options.Filter);

        var root = new JsonObject();
        var queue = new Queue<ConfigurationKey>(new[] { ConfigurationKey.Empty });

#if (!NETSTANDARD2_0)
        while (queue.TryDequeue(out var key))
        {
#else
        while (queue.Count > 0)
        {
            var key = queue.Dequeue();

#endif
            var name = key.GetKey();

            var current = root;
            var keys = key.GetSegments();
            for (var i = 0; i < keys.Length - 1; i++) current = current[keys[i]]!.AsObject();

            var children = configurationDictionary.GetChildKeys(key);
            if (children.Count == 0)
            {
                if (configurationDictionary.TryGetValue(key, out var provider))
                    current.Add(name, options.Formatter.Format(key, provider!));
                continue;
            }

            if (name.Length > 0) current.Add(name, new JsonObject());
            foreach (var child in children) queue.Enqueue(child);
        }

        return root;
    }
}