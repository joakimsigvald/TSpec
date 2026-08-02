namespace TSpec.Internal.Specification;

/// <summary>
/// One piece of composed text and how it joins what stands before it: a line of its own at some
/// indentation, or a continuation of the line in progress, introduced by its binder.
/// </summary>
/// <remarks>
/// The pieces are kept apart rather than concatenated because their boundaries carry meaning —
/// what arrives as one piece is one expression, and layout moves such a piece whole rather than
/// breaking inside it. Flattening to a string first would throw that away.
/// </remarks>
internal readonly record struct TextUnit(string Text, int? Indentation, string Binder = " ")
{
    internal static TextUnit Line(string text, int indentation) => new(text, indentation);

    internal static TextUnit Word(string text, string binder) => new(text, null, binder);

    internal bool StartsLine => Indentation is not null;
}

/// <summary>
/// A specification composed but not laid out: every word it will say is settled, and no line break
/// is. Whatever else has to happen to the text — a heading dropping the word it has already said,
/// an item indenting its fence — happens here, while the text can still be measured honestly.
/// Layout is applied last, by <see cref="Render"/>, against the width the text will really occupy.
/// </summary>
internal sealed record ComposedText(IReadOnlyList<TextUnit> Units, bool OpensSentence = true)
{
    /// <summary>
    /// The text without its opening word, where something above it has already said that word.
    /// What remains no longer opens the sentence — the heading did — so it keeps its own case.
    /// </summary>
    internal ComposedText Without(string? word)
    {
        if (word is null || Units.Count == 0)
            return this;

        var opening = Units[0];
        if (!opening.StartsLine)
            return this;

        // The word is sometimes a statement of its own — a bare "Then", with the claim continuing
        // the line it opened — and sometimes the first word of one. Alone with nothing after it, it
        // is the whole of what there is to say and stays.
        if (opening.Text == word)
        {
            if (Units.Count == 1)
                return this;

            // What continued the removed word's line now begins one, so it takes no binder — and is
            // measured without the space that binder would have put in front of it.
            var next = Units[1];
            return new(
                [next.StartsLine ? next : TextUnit.Line(next.Text, 0), .. Units.Skip(2)],
                OpensSentence: false);
        }

        return opening.Text.StartsWith($"{word} ", StringComparison.Ordinal)
            ? new(
                [opening with { Text = opening.Text[(word.Length + 1)..] }, .. Units.Skip(1)],
                OpensSentence: false)
            : this;
    }

    /// The text laid out for the width it is written into. The last thing done to it.
    internal string Render(int maxLineLength)
    {
        var text = new TextBuilder(maxLineLength);
        foreach (var unit in Units)
            text.Add(unit);
        return text.Build(OpensSentence);
    }
}
