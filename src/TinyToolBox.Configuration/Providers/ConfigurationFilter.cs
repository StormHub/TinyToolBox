using System.Collections.Immutable;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.EnvironmentVariables;
using TinyToolBox.Configuration.Keys;

namespace TinyToolBox.Configuration.Providers;

internal sealed class ConfigurationFilter
{
    private readonly ImmutableDictionary<Type, Func<KeyValuePair<ConfigurationKey, IConfigurationProvider>, bool>> _filters;

    public ConfigurationFilter() 
        : this(new [] 
              {
                 new KeyValuePair<Type, Func<KeyValuePair<ConfigurationKey, IConfigurationProvider>, bool>>(
                     typeof(EnvironmentVariablesConfigurationProvider), NoTopLevelEnvironmentVariables)
              })
    { }

    public ConfigurationFilter(IEnumerable<KeyValuePair<Type, Func<KeyValuePair<ConfigurationKey, IConfigurationProvider>, bool>>> filters)
    {
        _filters = filters.ToImmutableDictionary();
    }

    public IEnumerable<KeyValuePair<ConfigurationKey, IConfigurationProvider>> Apply(
        IEnumerable<KeyValuePair<ConfigurationKey, IConfigurationProvider>> source) => 
            source.Where(pair => 
                _filters
                    .Where(x => x.Key.IsAssignableFrom(pair.Value.GetType()))
                    .All(filter => filter.Value.Invoke(pair)));

    private static bool NoTopLevelEnvironmentVariables(KeyValuePair<ConfigurationKey, IConfigurationProvider> pair) =>
        pair.Value is EnvironmentVariablesConfigurationProvider && pair.Key.Depth() > 1;
}
