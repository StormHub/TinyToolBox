using Microsoft.Extensions.Configuration;
using TinyToolBox.Configuration.Keys;

namespace TinyToolBox.Configuration.Providers;

internal sealed class ConfigurationFormatter
{
    private readonly Dictionary<Type, Func<IConfigurationProvider, string>> _mappings;

    public ConfigurationFormatter() 
    {
        _mappings = new Dictionary<Type, Func<IConfigurationProvider, string>>();
    }

    public void Add<T>(Func<T, string> formatter) where T : IConfigurationProvider
    {
        var mapping = (IConfigurationProvider provider) => formatter.Invoke((T)provider);

#if (NETSTANDARD2_0)
        if (!_mappings.ContainsKey(typeof(T)))
        {
            _mappings.Add(typeof(T), mapping);
        }
        else 
        {
            _mappings[typeof(T)] = mapping;
        }
#else
        if (!_mappings.TryAdd(typeof(T), mapping))
        {
            _mappings[typeof(T)] = mapping;
        }
#endif
    }

    public string Format(KeyValuePair<ConfigurationKey, IConfigurationProvider> pair) => Format(pair.Key, pair.Value);

    public string Format(ConfigurationKey key, IConfigurationProvider provider)
    {
        // Overrides
        if (_mappings.TryGetValue(provider.GetType(), out var formatter))
        {
            var result = formatter.Invoke(provider);
            if (!string.IsNullOrEmpty(result))
            {
                return result;
            }
        }

        // Chained
        if (provider is ChainedConfigurationProvider chained)
        {
            return FormatChained(key, chained);
        }

        return FormatDefault(provider);
    }

    private string FormatChained(ConfigurationKey key, ChainedConfigurationProvider chainedConfigurationProvider)
    {
        var stack = GetProviders(key, chainedConfigurationProvider);
#if (!NETSTANDARD2_0)
        if (stack.TryPop(out var provider))
        {
#else
        if (stack.Count > 0)
        {
            var provider = stack.Pop();
#endif
            if (provider is not ChainedConfigurationProvider)
            {
                string? result = default;
                if (_mappings.TryGetValue(provider.GetType(), out var formatter))
                {
                    result = formatter(provider);
                }

                if (string.IsNullOrEmpty(result))
                {
                    result = FormatDefault(provider);
                }

                return $"{result} (Chained:{stack.Count + 1})";
            }
        }

        return FormatDefault(chainedConfigurationProvider);
    }

    private static Stack<IConfigurationProvider> GetProviders(ConfigurationKey key, ChainedConfigurationProvider chainedConfigurationProvider)
    {
        static IConfigurationProvider? ChainOf(ConfigurationKey key, ChainedConfigurationProvider chained) =>
            chained.Configuration is IConfigurationRoot chainedRoot ? chainedRoot.GetProvider(key) : null;

        var stack = new Stack<IConfigurationProvider>();
        var provider = ChainOf(key, chainedConfigurationProvider);
        while (provider != null)
        {
            stack.Push(provider);
            if (provider is not ChainedConfigurationProvider chained)
            {
                break;
            }

            provider = ChainOf(key, chained);
        }

        return stack;
    }

    private static string FormatDefault(IConfigurationProvider provider) =>
        provider switch
        {
            // Abstract types
            FileConfigurationProvider file => $"{file.Source.Path} ({(file.Source.Optional ? "Optional" : "Required")})",
            StreamConfigurationProvider stream => $"{FallbackFormatter(provider)} ({stream.Source.Stream!.GetType().Name})",

            // Default
            _ => FallbackFormatter(provider),
        };

    private static string FallbackFormatter(IConfigurationProvider provider) => 
        provider.ToString() ?? provider.GetType().Name;
}
