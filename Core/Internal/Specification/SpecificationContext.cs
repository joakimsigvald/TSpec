using System.Runtime.CompilerServices;
using Xunit.Sdk;

namespace TSpec.Internal.Specification;

/// <summary>
/// Thread-bound facade over the specification machinery: forwards each
/// pipeline step to its phrase class (which records it for deferred
/// rendering), tracks tagged value assignments, and reports assertion
/// failures with the full specification attached.
/// </summary>
internal class SpecificationContext : IAssertSpecificationContext
{
    private static readonly AsyncLocal<SpecificationContext?> _currentAssertionContext = new();

    /// <summary>
    /// The context of the executing test. Created by the Spec constructor; when TSpec.Assert
    /// is used standalone (without a Spec), a detached context is created on first use.
    /// </summary>
    internal static IAssertSpecificationContext Current => _currentAssertionContext.Value ??= new();

    private readonly SpecificationRecording _recording;
    private readonly SetupPhrases _setup;
    private readonly ActionPhrases _action;
    private readonly AssertionPhrases _assertion;
    private readonly SpecificationAssignments _assignments = new();
    private string? _subjectDescription;

    private SpecificationContext()
    {
        var textBuilder = new TextBuilder();
        _recording = new(textBuilder);
        _setup = new(_recording, textBuilder);
        _action = new(_recording, textBuilder);
        _assertion = new(_recording, textBuilder);
    }

    /// <summary>
    /// Register the subject of the current assertion chain, as declared by the
    /// Then/And overload that received it. Pass null when the wrapper takes no
    /// subject, so a previously registered subject cannot leak into it.
    /// </summary>
    public void SetSubject(string? subjectExpr) => _subjectDescription = subjectExpr?.ParseValue();

    internal static string? PendingSubject => _currentAssertionContext.Value?._subjectDescription;

    public override string ToString() => _recording.ToString();

    internal void AddWhen(string actExpr) => _action.AddWhen(actExpr);

    internal void AddAfter(string setUpExpr) => _action.AddAfter(setUpExpr);

    internal void AddBefore(string tearDownExpr) => _action.AddBefore(tearDownExpr);

    internal void AddTap(string expr) => _action.AddTap(expr);

    internal void AddGiven(string valueExpr, For scope) => _setup.AddGiven(valueExpr, scope);

    internal void AddUsing(string valueExpr, For scope, bool owned = false)
        => _setup.AddUsing(valueExpr, scope, owned);

    internal void AddUsing(Func<bool> shouldRender, string valueExpr, For scope)
        => _setup.AddUsing(shouldRender, valueExpr, scope);

    internal void AddUsingConversion<TTarget, TSource>(For scope, Func<string> describeSequence)
        => _setup.AddUsingConversion<TTarget, TSource>(scope, describeSequence);

    internal void AddUsingFactory<TTarget>(For scope, string generateExpr)
        => _setup.AddUsingFactory<TTarget>(scope, generateExpr);

    internal void AddGiven<TValue>(string setupExpr, bool isCustomExpression, string? article = null)
        => _setup.AddGiven<TValue>(setupExpr, isCustomExpression, article);

    internal void AddGivenCount<TModel>(string count) => _setup.AddGivenCount<TModel>(count);

    internal void AddGivenThat(string customArrangementExpr)
        => _setup.AddGivenThat(customArrangementExpr);

    internal void AddMockSetup<TService>(string callExpr) => _setup.AddMockSetup<TService>(callExpr);

    internal void AddMockReturns(string? returnsExpr = null) => _setup.AddMockReturns(returnsExpr);

    internal void AddMockThrowsDefault<TService, TError>()
        => _setup.AddMockThrowsDefault<TService, TError>();

    internal void AddMockThrowsDefault<TService>(string expectedExpr)
        => _setup.AddMockThrowsDefault<TService>(expectedExpr);

    internal void AddMockThrows<TError>() => _setup.AddMockThrows<TError>();

    internal void AddMockThrows(string expectedExpr) => _setup.AddMockThrows(expectedExpr);

    internal void AddMockReturnsDefault<TService>(string returnsExpr)
        => _setup.AddMockReturnsDefault<TService>(returnsExpr);

    internal void TagIndex(Type type, int index, string tagName)
         => _assignments.TagIndex(type, index, tagName);

    internal void Assign(Type type, int index, object? value) => _assignments.Assign(type, index, value);

    internal void AddBecause(string reason) => _recording.SetBecause(reason);

    public void Assert(
    Action assert,
    string actual,
    string? expected,
    string verb)
    {
        _assertion.AddAssert(actual, verb, expected);
        try
        {
            _recording.SuppressRecording();
            assert();
            _recording.InciteRecording();
        }
        catch (XunitException ex)
        {
            var message = ex.Message;
            var innerTSpecTEx = GetExpectedException(ex.InnerException as XunitException);
            if (innerTSpecTEx is not null)
                message = $"{message}{Environment.NewLine}{innerTSpecTEx.Message}";
            var assignmentList = _assignments.ListAssignments();
            var specMessage = string.Join(
                Environment.NewLine, string.Empty, _recording, "----", assignmentList);
            throw new XunitException(message, new XunitException(specMessage));
        }
    }

    public void AddThen() => _assertion.AddThen();

    public void AddThat() => _assertion.AddThat();

    public void AddVerify<TService>(string expressionExpr) => _assertion.AddVerify<TService>(expressionExpr);

    public void AddWasInvoked<TService>(string? timesExpr) => _assertion.AddWasInvoked<TService>(timesExpr);

    public void AddWasInvoked<TService>(string method, string? timesExpr) => _assertion.AddWasInvoked<TService>(method, timesExpr);

    public void AddAssertThrows<TError>(string? binder = null) => _assertion.AddAssertThrows<TError>(binder);

    public void AddAssertThrows(string expectedExpr) => _assertion.AddAssertThrows(expectedExpr);

    public void AddAssertDoesNotThrow<TError>() => _assertion.AddAssertDoesNotThrow<TError>();

    public void AddAssert([CallerMemberName] string? assertName = null)
         => _assertion.AddAssert(assertName!);

    public void AddAssertConjunction(string conjunction) => _assertion.AddAssertConjunction(conjunction);

    // ----------- Lifecycle

    internal static SpecificationContext Create() => _currentAssertionContext.Value = new();

    internal static void Release() => _currentAssertionContext.Value = null;

    private static XunitException? GetExpectedException(XunitException? ex)
        => ex is null || ex.Message.StartsWith("Expected")
            ? ex
            : GetExpectedException(ex.InnerException as XunitException);
}
