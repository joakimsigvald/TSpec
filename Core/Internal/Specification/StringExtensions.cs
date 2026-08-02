namespace TSpec.Internal.Specification;

internal static class StringExtensions
{
    internal static string AsWords(this string str, VerbalizationStrategy verbalizationStrategy = VerbalizationStrategy.None)
    {
        var words = str.ToWords();
        if (verbalizationStrategy == VerbalizationStrategy.PresentSingularS)
            // "have" is the one irregular an assertion verb reaches; the rest is the shared rule.
            words[0] = words[0].EndsWith("ve") ? $"{words[0][..^2]}s" : AddS(words[0]);
        return string.Join(' ', words);
    }

    /// <summary>
    /// The plural of a noun. English spells noun plurals and third-person verbs alike, so this is
    /// the same rule <see cref="AsWords"/> applies for <c>PresentSingularS</c> rather than a second
    /// copy of it. Anything that is not a plain identifier was never a noun to inflect — a generic
    /// or an array has no spelling that reads as a plural — and is returned as written.
    /// </summary>
    /// <remarks>
    /// Regular nouns only. An irregular one comes out as though it were regular, and no author can
    /// fix that by renaming their domain type; it is accepted because it is the same kind of wrong
    /// as the bare singular it replaces, and far rarer.
    /// </remarks>
    internal static string Pluralize(this string noun)
        => IsIdentifier(noun) ? AddS(noun) : noun;

    /// <summary>
    /// A type named alongside a count reads as a plural — "two Rooms". <c>One</c> is a count too and
    /// stays singular, which is why the factories are listed rather than inferred from the count.
    /// </summary>
    internal static string CountedBy(this string typeName, string factory)
        => _pluralFactories.Contains(factory) ? typeName.Pluralize() : typeName;

    private static readonly HashSet<string> _pluralFactories =
        ["Zero", "Two", "Three", "Four", "Five", "Some", "Many", "AnyNumberOf"];

    /// <summary>
    /// A sibilant needs a syllable of its own, and <c>-y</c> becomes <c>-ies</c> only after a
    /// consonant — "query" gives "queries" where "key" gives "keys".
    /// </summary>
    private static string AddS(string word)
        => word.Length > 1 && word.EndsWith('y') && !IsVowel(word[^2]) ? $"{word[..^1]}ies"
        : word.EndsWith('s') || word.EndsWith('x') || word.EndsWith('z')
            || word.EndsWith("ch") || word.EndsWith("sh") ? $"{word}es"
        : $"{word}s";

    private static bool IsVowel(char c) => "aeiouAEIOU".Contains(c);

    /// <summary>
    /// Breaks an identifier into words. An underscore separates clauses rather than words, so
    /// <c>GivenASnake_WithWings</c> reads "given a snake, with wings".
    /// </summary>
    internal static string[] ToWords(this string str)
    {
        if (string.IsNullOrWhiteSpace(str))
            return [string.Empty];

        List<string> words = [];
        foreach (var clause in str.Split('_', StringSplitOptions.RemoveEmptyEntries))
        {
            if (words.Count > 0)
                words[^1] += ",";
            words.AddRange(MergeAcronyms(SplitWords(clause)).Select(Normalize));
        }
        return [.. words];
    }

    /// <summary>
    /// Splitting at every capital tears an acronym into letters, so a run of them is put back
    /// together: <c>HTTPStatus</c> gives H, T, T, P, Status and reads "HTTP status". A lone letter
    /// is a word in its own right — the "a" of <c>GivenASnake</c>.
    /// </summary>
    private static IEnumerable<string> MergeAcronyms(IEnumerable<string> words)
    {
        var acronym = string.Empty;
        foreach (var word in words)
        {
            if (word.Length == 1)
            {
                acronym += word;
                continue;
            }
            if (acronym.Length > 0)
                yield return acronym;

            acronym = string.Empty;
            yield return word;
        }
        if (acronym.Length > 0)
            yield return acronym;
    }

    /// An acronym keeps the case that identifies it as one; every other word is lowered.
    private static string Normalize(string word)
        => word.Length > 1 && word.All(char.IsUpper) ? word : word.ToLower();

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

    /// <summary>
    /// A class or method name as a document heading: <c>GivenNoSuchRoom</c> reads
    /// "Given no such room". A branch path is dotted, and each segment becomes its own sentence.
    /// </summary>
    internal static string AsHeading(this string name)
        => string.Join(". ", name.Split('.').Select(part => part.AsWords().Capitalize()));

    /// <summary>
    /// A subject name as a document title: <c>MyHotel</c> reads "My Hotel", and the dotted
    /// <c>MyHotel.Core</c> reads "My Hotel Core". The dot separates parts of one name here, unlike
    /// in <see cref="AsHeading"/>, where a branch path really is a sequence of sentences.
    /// </summary>
    internal static string AsTitle(this string name)
        => string.Join(' ', name.Split('.')
            .SelectMany(part => part.ToWords())
            .Select(word => word.Capitalize()));

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