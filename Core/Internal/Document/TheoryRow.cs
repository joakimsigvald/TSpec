using System.Collections;
using System.Reflection;
using TSpec.Internal.Specification;
using Xunit.v3;

namespace TSpec.Internal.Document;

/// <summary>
/// The <c>[InlineData]</c> row a theory is running: the parameter names it fills, the values it
/// fills them with, and where the row was declared. The document lays these out as a table under
/// the requirement, so a theory states its data rather than only its parameter names.
/// </summary>
/// <remarks>
/// <c>InlineData</c> only. Data living in a separate file is not specification, so a theory fed by
/// <c>MemberData</c> or <c>ClassData</c> reads no row and goes on rendering as it always has.
/// </remarks>
internal sealed record TheoryRow(
    int Index, IReadOnlyList<string> Headers, IReadOnlyList<string> Values)
{
    internal static TheoryRow? Read()
    {
        if (TestContext.Current.TestMethod is not IXunitTestMethod method
            || TestContext.Current.Test is not IXunitTest test)
            return null;

        ParameterInfo[] parameters = [.. method.Parameters];
        InlineDataAttribute[] declared = [.. method.DataAttributes.OfType<InlineDataAttribute>()];
        if (parameters.Length == 0
            || parameters.Length != test.TestMethodArguments.Length
            || declared.Length == 0
            || declared.Length != method.DataAttributes.Count)
            return null;

        var collected = Collected(parameters);
        var running = Spelling(test.TestMethodArguments, collected);
        var index = Array.FindIndex(
            declared, row => Spelling(row.Data, collected).SequenceEqual(running));
        return index < 0 ? null : new(
            index,
            [.. parameters.Select(parameter => parameter.Name ?? string.Empty)],
            [.. test.TestMethodArguments.Select(ObjectExtensions.FormatValue)]);
    }

    /// The type of a trailing <c>params</c> array, which the runner hands back collected.
    private static Type? Collected(ParameterInfo[] parameters)
        => parameters[^1] is { ParameterType.IsArray: true } last
            && last.IsDefined(typeof(ParamArrayAttribute))
                ? last.ParameterType
                : null;

    /// <summary>
    /// What makes a run of the theory the same row as one of its declarations. An author may spell
    /// a <c>params</c> argument either as loose values or as one array, and the runner always hands
    /// it back as an array — so both spellings are flattened before they are compared.
    /// </summary>
    private static string[] Spelling(IReadOnlyList<object?> values, Type? collected)
        => [.. Flatten(values, collected).Select(ObjectExtensions.FormatValue)];

    private static IEnumerable<object?> Flatten(IReadOnlyList<object?> values, Type? collected)
    {
        for (var at = 0; at < values.Count; at++)
            if (at == values.Count - 1 && collected is not null && values[at]?.GetType() == collected)
                foreach (var element in (IEnumerable)values[at]!)
                    yield return element;
            else
                yield return values[at];
    }
}
