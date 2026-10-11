using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using TSpec.Internal.Specification;

namespace TSpec.Assert.Continuations;

/// <summary>
/// The four independent facts about an assertion in a chain. They combine freely — name no
/// combinations here; every consumer tests one flag at a time with HasFlag.
/// Either lasts from either to or: assertions evaluate eagerly, so an alternative can hold its failure
/// only while it is known that another follows, and the one after or decides.
/// </summary>
[Flags]
internal enum ConstraintState
{
    Normal = 0,
    Inverted = 1,
    Either = 2,
    Succeeded = 4,
    Failed = 8,
};

/// <summary>
/// Base record holding the state of an assertion, such as the description of the asserted value and the evaluation state
/// </summary>
public record Constraint
{
    internal string ActualExpr { get; set; } = "!UNDESCRIBED!";
    internal string? AuxiliaryVerb { get; set; }
    internal ConstraintState State { get; set; } = default;
    internal IReadOnlyList<Failure> Failures { get; set; } = [];
    internal bool WasInverted { get; set; }
}

/// <summary>
/// Base class for object that allows a chain of assertions to be made on the provided value
/// </summary>
public abstract record Constraint<TActual, TContinuation>
    : Constraint
    where TContinuation : Constraint<TActual, TContinuation>, new()
{
    internal Constraint() : base() => AuxiliaryVerb = typeof(TContinuation).Name.ToWords()[0];

    /// <summary>
    /// Invert the following assertion
    /// </summary>
    /// <returns>A continuation for making further assertions on the value</returns>
    [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Special convention of binding words")]
    public TContinuation not
        => new()
        {
            Actual = Actual,
            ActualExpr = ActualExpr,
            State = State | ConstraintState.Inverted,
            Failures = Failures,
            AuxiliaryVerb = $"{AuxiliaryVerb} not"
        };

    /// <summary>
    /// Asserts that the value is one of the given values
    /// </summary>
    /// <param name="values">The values that the asserted value is expected to be one of</param>
    /// <param name="expectedExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns>A continuation for making further assertions on the value</returns>
    public ContinueWith<TContinuation> OneOf(
        TActual[] values,
        [CallerArgumentExpression(nameof(values))] string? expectedExpr = null)
        => Assert(
            $"[{string.Join(", ", values.Select(v => Describe(v)))}]",
            actual => Xunit.Assert.Contains(actual, values),
            expectedExpr!).And();

    /// <summary>
    /// Used to provide alternatives, one of which has to pass for the test to pass: separated by '_',
    /// with 'or' before the last
    /// </summary>
    [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Special convention of binding words")]
    public TContinuation either
    {
        get
        {
            if (State.HasFlag(ConstraintState.Inverted))
                throw new SetupFailed("Either-or cannot be used after not");
            SpecificationContext.Current.OpenEither(ActualExpr);
            // or.either is _ written out: one more alternative before the last.
            if (FollowsAnAlternative)
                SpecificationContext.Current.AddAssertConjunction("_");
            return new()
            {
                Actual = Actual,
                ActualExpr = ActualExpr,
                State = State | ConstraintState.Either,
                Failures = Failures,
                AuxiliaryVerb = AuxiliaryVerb,
            };
        }
    }

    // Right after or, an earlier alternative has been decided, which an assertion after and never has.
    private bool FollowsAnAlternative
        => State.HasFlag(ConstraintState.Succeeded) || State.HasFlag(ConstraintState.Failed);

    /// <summary>
    /// Ends an either with what to do when none of its alternatives holds, for the failure to say. It ends the
    /// test too, and the specification leaves it out
    /// </summary>
    /// <param name="what">What to do to make the test pass</param>
    /// <param name="because">Why it matters, phrased to read after the word "because"</param>
    /// <example>
    /// <code>
    /// files.Is().either.Empty().or.Fix("move the file into its module", because: "a host only composes");
    /// </code>
    /// </example>
    public void Fix(string what, string? because = null)
    {
        if (!IsAfterOr)
            throw new SetupFailed("Fix is the last alternative of an either: write it after or");

        SpecificationContext.Current.Fix(() => Decide(because is null ? what : $"{what}, because {because}"));
    }

    // After or the either is closed and an earlier alternative decided; after _ it is still open.
    private bool IsAfterOr
        => FollowsAnAlternative && !State.HasFlag(ConstraintState.Either) && !State.HasFlag(ConstraintState.Inverted);

    private void Decide(string fix)
    {
        if (!State.HasFlag(ConstraintState.Succeeded))
            throw Failure.Of(ActualExpr, Failures, fix);
    }

    internal static TContinuation Create(TActual? actual, string actualExpr)
        => new() { Actual = actual, ActualExpr = actualExpr.DescribeActual(SpecificationContext.PendingSubject) };

    internal TActual? Actual { get; set; }

    internal virtual ContinueWith<TContinuation> Value(
        TActual expected, string expectedExpr)
        => Assert(expected, actual => Xunit.Assert.Equal(expected, actual), expectedExpr, methodName: string.Empty)
        .And();

    private protected static Action<TActual?> NotNullAnd(Action<TActual> assert)
        => actual =>
        {
            Xunit.Assert.NotNull(actual);
            assert(actual);
        };

    private protected Action<TActual?> AssertNot(Action<TActual?> assert)
        => actual =>
        {
            try
            {
                assert(Actual);
            }
            catch (Xunit.Sdk.XunitException)
            {
                return;
            }
            Xunit.Assert.Fail();
        };

    private protected Constraint<TActual, TContinuation> Assert(
        object? expected,
        Action<TActual?> assert,
        string? expectedExpr = null,
        string auxVerb = "be",
        VerbalizationStrategy verbalizationStrategy = VerbalizationStrategy.None,
        [CallerMemberName] string? methodName = null)
    {
        var isInverted = State.HasFlag(ConstraintState.Inverted);
        if (!isInverted && verbalizationStrategy == VerbalizationStrategy.PresentSingularS)
            AuxiliaryVerb = string.Empty;
        return Assert(() =>
            {
                if (State.HasFlag(ConstraintState.Succeeded))
                    return;

                DoAssert();
                if (State.HasFlag(ConstraintState.Succeeded)
                    || State.HasFlag(ConstraintState.Either))
                    return;

                throw Failure.Of(ActualExpr, Failures);
            }, methodName!, expectedExpr, isInverted ? VerbalizationStrategy.None : verbalizationStrategy);

        void DoAssert()
        {
            var holds = Holds(out var xuex);
            State |= holds ? ConstraintState.Succeeded : ConstraintState.Failed;
            if (!holds)
                Failures = [.. Failures, new(GetExpectation(), Describe(Actual, methodName!), xuex)];
        }

        bool Holds(out Xunit.Sdk.XunitException? xuex)
        {
            xuex = null;
            try
            {
                assert(Actual);
                return !isInverted;
            }
            catch (Exception ex)
            {
                xuex = ex as Xunit.Sdk.XunitException;
                return isInverted;
            }
        }

        string GetExpectation() => $"{GetVerb()} {expected.InvariantText()}".Trim();

        string GetVerb() => $"{GetAuxVerb()} {methodName!.AsWords()}".Trim();

        string GetAuxVerb() =>
            ((isInverted ? "not " : string.Empty)
            + (verbalizationStrategy == VerbalizationStrategy.PresentSingularS ? string.Empty : auxVerb))
            .TrimEnd();
    }

    private protected virtual string Describe(TActual? value, string? methodName = null) => value.FormatValue();

    internal ContinueWith<TContinuation> And() => new(Continued());
    internal ContinueWithThat<TContinuation, TThat> AndThat<TThat>(TThat that)
        => new(Continued(), that, WasInverted || State.HasFlag(ConstraintState.Inverted));
    internal virtual TContinuation Continue() => (TContinuation)this;

    // Strings, collections and counts continue on a new instance, which must carry an either's state on to its or.
    private TContinuation Continued()
    {
        var continuation = Continue();
        continuation.State = State;
        continuation.Failures = Failures;
        return continuation;
    }

    private protected TContinuation Assert(
        Action assert,
        string methodName,
        string? expected,
        VerbalizationStrategy verbalizationStrategy = VerbalizationStrategy.None)
    {
        var verb = $"{AuxiliaryVerb} {methodName.AsWords(verbalizationStrategy)}".Trim();
        SpecificationContext.Current.Assert(assert, ActualExpr, expected, verb);
        var continuation = Continued();
        continuation.WasInverted = State.HasFlag(ConstraintState.Inverted);
        return continuation;
    }
}