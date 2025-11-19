using System.Collections;
using System.Collections.Immutable;
using Microsoft.Extensions.Configuration;

namespace TinyToolBox.Configuration.Keys;

internal sealed class ConfigurationDictionary(
    IEnumerable<KeyValuePair<ConfigurationKey, IConfigurationProvider>> keyValuePairs)
    : IEnumerable<KeyValuePair<ConfigurationKey, IConfigurationProvider>>
{
    private readonly ImmutableDictionary<ConfigurationKey, IConfigurationProvider> _keyDictionary = keyValuePairs.ToImmutableDictionary();

    public IEnumerator<KeyValuePair<ConfigurationKey, IConfigurationProvider>> GetEnumerator()
    {
        foreach (var key in _keyDictionary.Keys.Order(ConfigurationKey.Comparer))
            yield return new KeyValuePair<ConfigurationKey, IConfigurationProvider>(key, _keyDictionary[key]);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public IReadOnlyList<ConfigurationKey> GetChildKeys(ConfigurationKey parent) =>
        ChildrenOf(parent)
            .ToHashSet()
            .Order(ConfigurationKey.Comparer)
            .ToList();

    private IEnumerable<ConfigurationKey> ChildrenOf(ConfigurationKey parent)
    {
        foreach (var key in _keyDictionary.Keys.Select(x => x.ChildOf(parent)))
            if (key.HasValue)
                yield return key.Value;
    }

    public bool TryGetValue(ConfigurationKey key, out IConfigurationProvider? provider) => _keyDictionary.TryGetValue(key, out provider);
}