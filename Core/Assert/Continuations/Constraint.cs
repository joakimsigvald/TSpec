using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using TSpec.Internal.Specification;

namespace TSpec.Assert.Continuations;

[Flags]
internal enum ConstraintState
{
    Normal = 0,
    Inverted = 1,
    Either = 2,
    InvertedEither = 3,
    Succeeded = 4,
    EitherSucceeded = 6,
    InvertedEitherSucceeded = 7,
    Failed = 8,
    EitherFailed = 10,
    InvertedEitherFailed = 11,
};

/// <summary>
/// Base record holding the state of an assertion, such as the description of the asserted value and the evaluation state
/// </summary>
public record Constraint
{
    internal string ActualExpr { get; set; } = "!UNDESCRIBED!";
    internal string? AuxiliaryVerb { get; set; }
    internal ConstraintState State { get; set; } = default;
    internal Exception? Exception { get; set; }
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
    /// Used to provide two assertions, separated by 'or', one of which has to pass for the test to pass
    /// </summary>
    [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Special convention of binding words")]
    public TContinuation either
    {
        get
        {
            if (State.HasFlag(ConstraintState.Inverted))
                throw new SetupFailed("Either-or cannot be used after not");
            return new()
            {
                Actual = Actual,
                ActualExpr = ActualExpr,
                State = State | ConstraintState.Either,
                AuxiliaryVerb = $"{AuxiliaryVerb} either",
            };
        }
    }

    internal static TContinuation Create(TActual? actual, string actualExpr)
        => new() { Actual = actual, ActualExpr = actualExpr.ParseActual(SpecificationContext.PendingSubject) };

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

                throw Exception!;
            }, methodName!, expectedExpr, isInverted ? VerbalizationStrategy.None : verbalizationStrategy);

        void DoAssert()
        {
            Xunit.Sdk.XunitException? xuex = null;
            if (State.HasFlag(ConstraintState.Failed))
                State &= ~ConstraintState.Either;
            try
            {
                assert(Actual);
                State |= isInverted ? ConstraintState.Failed : ConstraintState.Succeeded;
            }
            catch (Exception ex)
            {
                State |= isInverted ? ConstraintState.Succeeded : ConstraintState.Failed;
                xuex = ex as Xunit.Sdk.XunitException;
            }
            if (State.HasFlag(ConstraintState.Failed))
                Exception ??= GetException(xuex);
        }

        Xunit.Sdk.XunitException GetException(Xunit.Sdk.XunitException? innerEx)
            => new($"Expected {ActualExpr} to {GetExpectation()} but found {Describe(Actual, methodName!)}", innerEx);

        string GetExpectation() => $"{GetVerb()} {expected}".Trim();

        string GetVerb() => $"{GetAuxVerb()} {methodName!.AsWords()}".Trim();

        string GetAuxVerb() =>
            ((isInverted ? "not " : string.Empty)
            + (verbalizationStrategy == VerbalizationStrategy.PresentSingularS ? string.Empty : auxVerb))
            .TrimEnd();
    }

    private protected virtual string Describe(TActual? value, string? methodName = null) => value.FormatValue();

    internal ContinueWith<TContinuation> And() => new(Continue());
    internal ContinueWithThat<TContinuation, TThat> AndThat<TThat>(TThat that) => new(Continue(), that);
    internal virtual TContinuation Continue() => (TContinuation)this;

    private protected TContinuation Assert(
        Action assert,
        string methodName,
        string? expected,
        VerbalizationStrategy verbalizationStrategy = VerbalizationStrategy.None)
    {
        var verb = $"{AuxiliaryVerb} {methodName.AsWords(verbalizationStrategy)}".Trim();
        SpecificationContext.Current.Assert(assert, ActualExpr, expected, verb);
        return Continue();
    }
}