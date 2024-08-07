﻿using Microsoft.Extensions.Configuration;

namespace TinyToolBox.Configuration.Keys;

internal readonly struct ConfigurationKey(string path) : IEquatable<ConfigurationKey>
{
    internal static readonly ConfigurationKey Empty = new(string.Empty);
    private static readonly char KeyDelimiter = ConfigurationPath.KeyDelimiter[0];

    internal sealed class KeyComparer : IComparer<ConfigurationKey>
    {
        public int Compare(ConfigurationKey x, ConfigurationKey y) => ConfigurationKeyComparer.Instance.Compare(x.Path, y.Path);
    }

    public static KeyComparer Comparer { get; } = new();

    public string Path { get; } = path.TrimStart(KeyDelimiter);

    public string GetKey() => ConfigurationPath.GetSectionKey(Path);

    public string[] GetSegments() => Path.Split(KeyDelimiter);

    public ConfigurationKey? ChildOf(ConfigurationKey parent)
    {
        if (parent == Empty) return Segment(Path, 0);

        if (Path.Length > parent.Path.Length
            && Path.StartsWith(parent, StringComparison.OrdinalIgnoreCase)
            && Path[parent.Path.Length] == KeyDelimiter)
            return ConfigurationPath.Combine(parent, Segment(Path, parent.Path.Length + 1));

        return null;
    }

    private static string Segment(string path, int prefixLength)
    {
        var indexOf = path.IndexOf(ConfigurationPath.KeyDelimiter, prefixLength, StringComparison.OrdinalIgnoreCase);

        return indexOf < 0
            ? path.Substring(prefixLength)
            : path.Substring(prefixLength, indexOf - prefixLength);
    }

    internal int Depth() => Path.Count(x => x == KeyDelimiter) + 1;

    public override bool Equals(object? obj) => obj is ConfigurationKey key && Equals(key);

    public bool Equals(ConfigurationKey other) => Equals(other, StringComparison.OrdinalIgnoreCase);

    public bool Equals(ConfigurationKey other, StringComparison comparisonType) => string.Equals(Path, other.Path, comparisonType);

    public override int GetHashCode() => StringComparer.OrdinalIgnoreCase.GetHashCode(Path);

    public override string ToString() => Path;

    public static bool operator ==(ConfigurationKey left, ConfigurationKey right) => left.Equals(right);

    public static bool operator !=(ConfigurationKey left, ConfigurationKey right) => !left.Equals(right);

    public static string operator +(string left, ConfigurationKey right) => string.Concat(left, right.ToString());

    public static string operator +(ConfigurationKey left, string? right) => string.Concat(left.ToString(), right);

    public static ConfigurationKey operator +(ConfigurationKey left, ConfigurationKey right) => left.Add(right);

    private ConfigurationKey Add(ConfigurationKey other) => new(Path + other.Path);

    public static implicit operator ConfigurationKey(string s) => FromString(s);

    public static implicit operator string(ConfigurationKey key) => key.ToString();

    private static ConfigurationKey FromString(string s) => new(s);
}