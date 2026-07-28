namespace TSpec.Internal.Specification;

internal static class StringExtensions
{
    internal static string AsWords(this string str, VerbalizationStrategy verbalizationStrategy = VerbalizationStrategy.None)
    {
        var words = str.ToWords();
        if (verbalizationStrategy == VerbalizationStrategy.PresentSingularS)
        {
            var firstWord = words[0];
            if (firstWord.EndsWith('y'))
                words[0] = $"{firstWord[..^1]}ies";
            else if (firstWord.EndsWith("ve"))
                words[0] = $"{firstWord[..^2]}s";
            else if (firstWord.EndsWith('s') || firstWord.EndsWith('x') || firstWord.EndsWith('z')
                || firstWord.EndsWith("ch") || firstWord.EndsWith("sh"))
                words[0] = $"{firstWord}es";
            else
                words[0] = $"{firstWord}s";
        }
        return string.Join(' ', words);
    }

    internal static string[] ToWords(this string str)
        => string.IsNullOrWhiteSpace(str)
        ? [string.Empty]
        : [.. SplitWords(str).Select(word => word.ToLower())];

    /// <summary>
    /// A tag reads as a name of its own in a specification, not as the variable it happens to be
    /// held in — so the field convention's leading underscore goes and the name is capitalized.
    /// Anything that is not a plain identifier is left alone: it was not a tag reference.
    /// </summary>
    internal static string AsTagName(this string tagExpr)
        => IsIdentifier(tagExpr) ? tagExpr.TrimStart('_').Capitalize() : tagExpr;

    private static bool IsIdentifier(string str)
        => !string.IsNullOrEmpty(str)
        && (char.IsLetter(str[0]) || str[0] == '_')
        && str.All(c => char.IsLetterOrDigit(c) || c == '_');

    internal static string NormalizeLineEndings(this string str)
        => str.Replace("\r\n", "\n").Replace('\r', '\n');

    /// Reduce a captured Times expression to its bare factory name, so that both the
    /// `using static Moq.Times;` form (`Once`) and the qualified form (`Times.Once()`)
    /// render alike: "Once", "Never", "Exactly(2)".
    internal static string NormalizeTimes(this string? expr)
    {
        if (string.IsNullOrWhiteSpace(expr))
            return string.Empty;
        var trimmed = expr.Trim();
        if (trimmed.StartsWith("Times."))
            trimmed = trimmed["Times.".Length..];
        if (trimmed.EndsWith("()"))
            trimmed = trimmed[..^2];
        return trimmed;
    }

    internal static string Capitalize(this string str)
        => string.IsNullOrWhiteSpace(str)
        ? string.Empty
        : str[..1].ToUpper() + str[1..];

    private static IEnumerable<string> SplitWords(string camelCase)
    {
        int fromIndex = 0;
        for (var toIndex = 0; toIndex < camelCase.Length; toIndex++)
        {
            if (!char.IsUpper(camelCase[toIndex]))
                continue;
            if (toIndex > fromIndex)
                yield return camelCase[fromIndex..toIndex];
            fromIndex = toIndex;
        }
        yield return camelCase[fromIndex..];
    }
}