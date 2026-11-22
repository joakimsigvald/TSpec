using TSpec.Internal.Specification.ExpressionParsing.Expressions;

namespace TSpec.Internal.Specification.ExpressionParsing.Describe;

/// <summary>
/// The assertion <c>Has().All</c> runs on each item, as the specification states it of the item:
/// <c>project => project.Files.Has().None(IsLong)</c> reads "its Files has none is long". The item keeps
/// its name where it is no member's receiver, and a shape this does not read is described as a value.
/// </summary>
internal sealed class ItemAssertionDescriber : Describer
{
    private static readonly string[] _entries = ["Is", "Has", "Does"];

    /// <summary>
    /// Whether the assertion is on the item itself and expects the same of every item: its arguments name
    /// neither the item nor its index, so its failures read as one expectation
    /// </summary>
    internal static bool ExpectsTheSameOfEachItem(Expr expr)
        => expr.WithoutNoise() is Lambda { Params: [var item, ..] } lambda
            && AssertionOf(lambda.Body) is { } assertion
            && assertion.Receiver.WithoutNoise() is Identifier receiver
            && receiver.Name == item
            && !assertion.Args.Any(arg => Names(arg, lambda.Params));

    /// <summary>
    /// The assertion's lines, each a step below the one before: an All on the item heads a For each of its
    /// own, the assertion on its items below it
    /// </summary>
    internal IReadOnlyList<string> Lines(Expr expr)
        => expr.WithoutNoise() is Lambda { Params: [var item, ..] } lambda && AssertionOf(lambda.Body) is { } assertion
            ? LinesOf(assertion, item)
            : [Value.Describe(expr)];

    protected override string Render(Expr expr) => string.Join(' ', Lines(expr));

    private IReadOnlyList<string> LinesOf(Assertion assertion, string item)
        => assertion is { Entry: "Has", Negated: false, Method: "All", Args: [var each] } && IsAnAssertion(each)
            ? [$"for each of {Receiver(assertion.Receiver, item)}:", .. Lines(each)]
            : [Words(assertion, item)];

    private static bool IsAnAssertion(Expr expr)
        => expr.WithoutNoise() is Lambda lambda && AssertionOf(lambda.Body) is not null;

    private static bool Names(Expr expr, IReadOnlyList<string> parameters)
        => expr.WithoutNoise() is Identifier { Name: var name } && parameters.Contains(name)
            || expr.Children.Any(child => Names(child, parameters));

    // A Fix ending an either is left out, as it is of an assertion on the whole; the general call below would
    // take it for an assertion it cannot read.
    private static Assertion? AssertionOf(Expr body)
        => body.WithoutNoise() switch
        {
            Call { Target: Member { Name: "Fix", Target: Member { Name: "or", Target: var alternative } } }
                => AssertionOf(alternative),
            Call { Target: Member { Name: "Is", Target: var receiver }, Args: [var value] }
                => new(receiver, "Is", Negated: false, Method: null, [value]),
            Call { Target: Member { Name: var method, Target: var entry }, Args: var args }
                => EntryOf(entry) is { } assertion ? assertion with { Method = method, Args = args } : null,
            _ => null,
        };

    // The Is(), Has() or Does() that starts an assertion, and a not or an either after it.
    private static Assertion? EntryOf(Expr expr)
        => expr.WithoutNoise() switch
        {
            Member { Name: "either", Target: var entry } => EntryOf(entry),
            Member { Name: "not", Target: var entry } when EntryOf(entry) is { Negated: false } assertion
                => assertion with { Negated = true },
            Call { Target: Member { Name: var name, Target: var receiver }, Args: [] } when _entries.Contains(name)
                => new(receiver, name, Negated: false, Method: null, []),
            _ => null,
        };

    private static string Words(Assertion assertion, string item)
        => string.Join(' ', new[] { Receiver(assertion.Receiver, item), Verb(assertion), Arguments(assertion) }
            .Where(part => part.Length > 0));

    // The item's own member reads as its: project.Files is its Files.
    private static string Receiver(Expr receiver, string item)
        => RootOf(receiver) == item && receiver.WithoutNoise() is not Identifier
            ? $"its {receiver.ToSource()[(item.Length + 1)..]}"
            : new ActualDescriber().Describe(receiver);

    private static string? RootOf(Expr expr)
        => expr.WithoutNoise() switch
        {
            Identifier identifier => identifier.Name,
            Member member => RootOf(member.Target),
            Call call => RootOf(call.Target),
            Generic generic => RootOf(generic.Target),
            _ => null,
        };

    // Does() reads in the third person, as an assertion of its own does: "contains", "does not contain".
    private static string Verb(Assertion assertion)
        => assertion switch
        {
            { Entry: "Does", Negated: false } => assertion.Method!.AsWords(VerbalizationStrategy.PresentSingularS),
            { Entry: "Does" } => $"does not {assertion.Method!.AsWords()}",
            _ => $"{assertion.Entry.ToLowerInvariant()}{(assertion.Negated ? " not" : "")} {assertion.Method?.AsWords()}"
                .TrimEnd(),
        };

    // Under Has(), an argument is a condition, and a method group reads as the words of its name.
    private static string Arguments(Assertion assertion)
        => string.Join(", ", assertion.Args.Select(arg => assertion.Entry == "Has" ? Condition(arg) : Value.Describe(arg)));

    private sealed record Assertion(Expr Receiver, string Entry, bool Negated, string? Method, IReadOnlyList<Expr> Args);
}
