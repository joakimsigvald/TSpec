using System.Runtime.CompilerServices;

namespace TSpec.Assert.Continuations.Enumerable;

/// <summary>
/// Object that allows assertions to be made on the provided dictionary
/// </summary>
/// <typeparam name="TKey">The type of the keys in the dictionary</typeparam>
/// <typeparam name="TValue">The type of the values in the dictionary</typeparam>
public record HasDictionary<TKey, TValue> : HasEnumerable<KeyValuePair<TKey, TValue>>
{
    /// <summary>
    /// Invert the following assertion, e.g. Has().no.Key(k)
    /// </summary>
    /// <returns>A continuation for making further assertions on the dictionary</returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Special convention of binding words")]
    public HasDictionaryContinuation<TKey, TValue> no
        => new()
        {
            Actual = Actual,
            ActualExpr = ActualExpr,
            State = State | ConstraintState.Inverted,
            AuxiliaryVerb = $"{AuxiliaryVerb} no"
        };

    /// <summary>
    /// Asserts that the dictionary contains the given key
    /// </summary>
    /// <param name="expected">The key that the dictionary is expected to contain</param>
    /// <param name="expectedExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns>A continuation for making further assertions on the dictionary</returns>
    public ContinueWith<HasDictionaryContinuation<TKey, TValue>> Key(
        TKey expected, [CallerArgumentExpression(nameof(expected))] string? expectedExpr = null)
    {
        Assert(Express(expectedExpr, expected),
            NotNullAnd(actual => Xunit.Assert.True(ContainsKey(actual, expected))),
            Express(expectedExpr, expected),
            "have");
        return new(ContinueDictionary());
    }

    /// <summary>
    /// Asserts that the dictionary contains the given value
    /// </summary>
    /// <param name="expected">The value that the dictionary is expected to contain</param>
    /// <param name="expectedExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns>A continuation for making further assertions on the dictionary</returns>
    public ContinueWith<HasDictionaryContinuation<TKey, TValue>> Value(
        TValue expected, [CallerArgumentExpression(nameof(expected))] string? expectedExpr = null)
    {
        Assert(Express(expectedExpr, expected),
            NotNullAnd(actual => Xunit.Assert.Contains(expected, actual.Select(pair => pair.Value))),
            Express(expectedExpr, expected),
            "have");
        return new(ContinueDictionary());
    }

    internal ContinueWithThat<HasDictionaryContinuation<TKey, TValue>, TValue> ValueForKey(
        TKey key, string? keyExpr)
    {
        TValue? value = default;
        Assert(Express(keyExpr, key),
            NotNullAnd(actual =>
            {
                Xunit.Assert.True(TryGetValue(actual, key, out value));
            }),
            Express(keyExpr, key),
            "have");
        return new(ContinueDictionary(), value!, State.HasFlag(ConstraintState.Inverted));
    }

    private static bool ContainsKey(IEnumerable<KeyValuePair<TKey, TValue>> actual, TKey key)
        => actual is IReadOnlyDictionary<TKey, TValue> dictionary
            ? dictionary.ContainsKey(key)
            : actual.Any(pair => EqualityComparer<TKey>.Default.Equals(pair.Key, key));

    private static bool TryGetValue(IEnumerable<KeyValuePair<TKey, TValue>> actual, TKey key, out TValue? value)
    {
        if (actual is IReadOnlyDictionary<TKey, TValue> dictionary)
            return dictionary.TryGetValue(key, out value);

        foreach (var pair in actual)
        {
            if (!EqualityComparer<TKey>.Default.Equals(pair.Key, key))
                continue;
            value = pair.Value;
            return true;
        }
        value = default;
        return false;
    }

    private protected HasDictionaryContinuation<TKey, TValue> ContinueDictionary()
        => new()
        {
            Actual = Actual,
            ActualExpr = ActualExpr,
        };
}
