﻿using Microsoft.Extensions.Configuration;
using TinyToolBox.Configuration.Providers;

namespace TinyToolBox.Configuration.Keys;

internal static class ConfigurationRootExtensions
{
    internal static ConfigurationDictionary AsDictionary(
        this IConfigurationRoot configurationRoot, 
        string[]? segments = null, 
        ConfigurationFilter? filter = null)
    {
        var pathSegments = segments?.Where(x => !string.IsNullOrEmpty(x)).ToArray();
        filter ??= new ConfigurationFilter();

        return new(filter.Apply(
            configurationRoot.Enumerate(
                pathSegments?.Any() ?? false ? configurationRoot.GetSection(ConfigurationPath.Combine(pathSegments)) : null)));
    }

    internal static IEnumerable<KeyValuePair<ConfigurationKey, IConfigurationProvider>> Enumerate(
        this IConfigurationRoot configurationRoot,
        IConfigurationSection? section = null)
    {
        IConfiguration configuration = section != null ? section : configurationRoot;

        foreach (var pair in configuration.AsEnumerable())
        {
            var configurationSection = configurationRoot.GetSection(pair.Key);
            var provider = configurationRoot.GetProvider(configurationSection.Path);
            if (provider != null)
            {
                yield return new KeyValuePair<ConfigurationKey, IConfigurationProvider>(configurationSection.Path, provider);
            }
        }
    }

    internal static IConfigurationProvider? GetProvider(this IConfigurationRoot configurationRoot, string key)
    {
        foreach (var provider in configurationRoot.Providers.Reverse())
        {
            if (provider.TryGet(key, out _))
            {
                return provider;
            }
        }

        return null;
    }
}
