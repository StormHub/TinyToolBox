using Microsoft.Extensions.Configuration;

namespace TinyToolBox.Configuration.Keys;

internal readonly struct ConfigurationKey : IEquatable<ConfigurationKey>
{
    internal static readonly ConfigurationKey Empty = new(string.Empty);
    internal static readonly char KeyDelimiter = ConfigurationPath.KeyDelimiter[0];

    internal sealed class KeyComparer : IComparer<ConfigurationKey>
    {
        public int Compare(ConfigurationKey x, ConfigurationKey y)
        {
            return ConfigurationKeyComparer.Instance.Compare(x.Path, y.Path);
        }
    }

    public static KeyComparer Comparer { get; } = new();

    public ConfigurationKey(string path)
    {
        Path = path.TrimStart(KeyDelimiter);
    }

    public string Path { get; }

    public string GetKey()
    {
        return ConfigurationPath.GetSectionKey(Path);
    }

    public string[] GetSegments()
    {
        return Path.Split(KeyDelimiter);
    }

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

    internal int Depth()
    {
        return Path.Count(x => x == KeyDelimiter) + 1;
    }

    public override bool Equals(object? obj)
    {
        return obj is not null && obj is ConfigurationKey key && Equals(key);
    }

    public bool Equals(ConfigurationKey other)
    {
        return Equals(other, StringComparison.OrdinalIgnoreCase);
    }

    public bool Equals(ConfigurationKey other, StringComparison comparisonType)
    {
        return string.Equals(Path, other.Path, comparisonType);
    }

    public override int GetHashCode()
    {
        return StringComparer.OrdinalIgnoreCase.GetHashCode(Path);
    }

    public override string ToString()
    {
        return Path;
    }

    public static bool operator ==(ConfigurationKey left, ConfigurationKey right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(ConfigurationKey left, ConfigurationKey right)
    {
        return !left.Equals(right);
    }

    public static string operator +(string left, ConfigurationKey right)
    {
        return string.Concat(left, right.ToString());
    }

    public static string operator +(ConfigurationKey left, string? right)
    {
        return string.Concat(left.ToString(), right);
    }

    public static ConfigurationKey operator +(ConfigurationKey left, ConfigurationKey right)
    {
        return left.Add(right);
    }

    public ConfigurationKey Add(ConfigurationKey other)
    {
        return new ConfigurationKey(Path + other.Path);
    }

    public static implicit operator ConfigurationKey(string s)
    {
        return FromString(s);
    }

    public static implicit operator string(ConfigurationKey key)
    {
        return key.ToString();
    }

    private static ConfigurationKey FromString(string s)
    {
        return new ConfigurationKey(s);
    }
}