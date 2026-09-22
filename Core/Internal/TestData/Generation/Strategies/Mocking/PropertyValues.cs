using System.Reflection;

namespace TSpec.Internal.TestData.Generation.Strategies.Mocking;

/// <summary>
/// The values a mock's properties keep, so a property answers as an object's does. One value per
/// address — the property and the arguments an indexer was reached with — written by a set or by the
/// first read no setup answered, and answering every read after it.
/// </summary>
internal sealed class PropertyValues
{
    private readonly List<KeptValue> _kept = [];

    internal bool TryRead(MethodInfo getter, IReadOnlyList<object?> indices, out object? value)
    {
        lock (_kept)
        {
            var kept = At(getter, indices);
            value = kept?.Value;
            return kept is not null;
        }
    }

    internal void Write(MethodInfo accessor, IReadOnlyList<object?> indices, object? value)
    {
        lock (_kept)
        {
            if (At(accessor, indices) is { } kept)
                kept.Value = value;
            else
                _kept.Add(new(PropertyAccess.PropertyOf(accessor), Address(indices), value));
        }
    }

    private static Func<object?, bool>[] Address(IReadOnlyList<object?> indices)
        => [.. indices.Select(ArgumentMatcher.EqualTo)];

    private KeptValue? At(MethodInfo accessor, IReadOnlyList<object?> indices)
    {
        var property = PropertyAccess.PropertyOf(accessor);
        return _kept.FirstOrDefault(kept => kept.Property == property && kept.IsAt(indices));
    }

    private sealed class KeptValue((Type, string) property, Func<object?, bool>[] address, object? value)
    {
        internal (Type, string) Property => property;

        internal object? Value { get; set; } = value;

        internal bool IsAt(IReadOnlyList<object?> indices)
            => indices.Count == address.Length
            && address.Select((matches, index) => matches(indices[index])).All(matched => matched);
    }
}
