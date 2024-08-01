using System.Collections;
using System.Collections.Immutable;
using Microsoft.Extensions.Configuration;

namespace TinyToolBox.Configuration.Keys;

internal sealed class ConfigurationDictionary : IEnumerable<KeyValuePair<ConfigurationKey, IConfigurationProvider>>
{
    private readonly ImmutableDictionary<ConfigurationKey, IConfigurationProvider> _keyDictionary;

    public ConfigurationDictionary(IEnumerable<KeyValuePair<ConfigurationKey, IConfigurationProvider>> keyValuePairs)
    {
        _keyDictionary = keyValuePairs.ToImmutableDictionary();
    }

    public IEnumerator<KeyValuePair<ConfigurationKey, IConfigurationProvider>> GetEnumerator()
    {
#if NET7_0_OR_GREATER
        var keys = _keyDictionary.Keys.Order(ConfigurationKey.Comparer).ToList();
#else
        var keys = _keyDictionary.Keys.ToList();
        keys.Sort(ConfigurationKey.Comparer);
#endif
        foreach (var key in keys)
            yield return new KeyValuePair<ConfigurationKey, IConfigurationProvider>(key, _keyDictionary[key]);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public IReadOnlyList<ConfigurationKey> GetChildKeys(ConfigurationKey parent)
    {
#if NET7_0_OR_GREATER
        return ChildrenOf(parent)
            .ToHashSet()
            .Order(ConfigurationKey.Comparer)
            .ToList();
#else
        var hashSet = new HashSet<ConfigurationKey>(ChildrenOf(parent));
        var list = hashSet.ToList();
        list.Sort(ConfigurationKey.Comparer);
        return list;
#endif
    }

    private IEnumerable<ConfigurationKey> ChildrenOf(ConfigurationKey parent)
    {
        foreach (var key in _keyDictionary.Keys.Select(x => x.ChildOf(parent)))
            if (key.HasValue)
                yield return key.Value;
    }

    public bool TryGetValue(ConfigurationKey key, out IConfigurationProvider? provider)
    {
        return _keyDictionary.TryGetValue(key, out provider);
    }
}