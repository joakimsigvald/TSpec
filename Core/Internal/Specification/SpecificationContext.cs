using System.Runtime.CompilerServices;
using Xunit.Sdk;

namespace TSpec.Internal.Specification;

internal class SpecificationContext : IAssertSpecificationContext
{
    [ThreadStatic]
    private static SpecificationContext? _currentAssertionContext;

    internal static IAssertSpecificationContext Current => _currentAssertionContext!;

    private SpecificationContext()
    {
    }

    private readonly SpecificationBuilder _builder = new();

    private readonly SpecificationAssignments _assignments = new();

    private string? _subjectDescription;

    /// <summary>
    /// Register the subject of the current assertion chain, as declared by the
    /// Then/And overload that received it. Pass null when the wrapper takes no
    /// subject, so a previously registered subject cannot leak into it.
    /// </summary>
    public void SetSubject(string? subjectExpr) => _subjectDescription = subjectExpr?.ParseValue();

    internal static string? PendingSubject => _currentAssertionContext?._subjectDescription;

    public override string ToString() => _builder.ToString();

    internal void AddWhen(string actExpr) => _builder.Add(() => _builder.AddWhen(actExpr));

    internal void AddAfter(string setUpExpr) => _builder.Add(() => _builder.AddAfter(setUpExpr));

    internal void AddBefore(string tearDownExpr) => _builder.Add(() => _builder.AddBefore(tearDownExpr));

    internal void AddGiven(string valueExpr, For scope) => _builder.Add(() => _builder.AddGiven(valueExpr, scope));

    internal void AddUsing(string valueExpr, For scope, bool owned = false)
        => _builder.Add(() => _builder.AddUsing(valueExpr, scope, owned));

    internal void AddUsing(Func<bool> shouldRender, string valueExpr, For scope)
        => _builder.Add(() =>
        {
            if (shouldRender())
                _builder.AddUsing(valueExpr, scope);
        });

    internal void AddUsingConversion<TTarget, TSource>(For scope, Func<string> describeSequence)
        => _builder.Add(() => _builder.AddUsingConversion<TTarget, TSource>(scope, describeSequence));

    internal void AddUsingFactory<TTarget>(For scope, string generateExpr)
        => _builder.Add(() => _builder.AddUsingFactory<TTarget>(scope, generateExpr));

    internal void AddGiven<TValue>(string setupExpr, bool isCustomExpression, string? article = null)
        => _builder.Add(() => _builder.AddGiven<TValue>(setupExpr, isCustomExpression, article));

    internal void AddGivenCount<TModel>(string count)
        => _builder.Add(() => _builder.AddGivenCount<TModel>(count));

    internal void AddGivenThat(string customArrangementExpr)
        => _builder.Add(() => _builder.AddGivenThat(customArrangementExpr));

    internal void AddMockSetup<TService>(string callExpr)
        => _builder.Add(() => _builder.AddMockSetup<TService>(callExpr));

    internal void AddMockReturns(string? returnsExpr = null)
        => _builder.Add(() => _builder.AddMockReturns(returnsExpr));

    internal void AddMockThrowsDefault<TService, TError>()
        => _builder.Add(_builder.AddMockThrowsDefault<TService, TError>);

    internal void AddMockThrowsDefault<TService>(string expectedExpr)
        => _builder.Add(() => _builder.AddMockThrowsDefault<TService>(expectedExpr));

    internal void AddMockThrows<TError>()
        => _builder.Add(_builder.AddMockThrows<TError>);

    internal void AddMockThrows(string expectedExpr)
        => _builder.Add(() => _builder.AddMockThrows(expectedExpr));

    internal void AddMockReturnsDefault<TService>(string returnsExpr)
         => _builder.Add(() => _builder.AddMockReturnsDefault<TService>(returnsExpr));

    internal void AddTap(string expr) => _builder.Add(() => _builder.AddTap(expr));

    internal void TagIndex(Type type, int index, string tagName)
         => _assignments.TagIndex(type, index, tagName);

    internal void Assign(Type type, int index, object? value) => _assignments.Assign(type, index, value);

    //---------------- Assertion

    public void Assert(
    Action assert,
    string actual,
    string? expected,
    string verb)
    {
        _builder.Add(() => _builder.AddAssert(actual, verb, expected));
        try
        {
            _builder.SuppressRecording();
            assert();
            _builder.InciteRecording();
        }
        catch (XunitException ex)
        {
            var message = ex.Message;
            var innerTSpecTEx = GetExpectedException(ex.InnerException as XunitException);
            if (innerTSpecTEx is not null)
                message = $"{message}{Environment.NewLine}{innerTSpecTEx.Message}";
            var assignmentList = _assignments.ListAssignments();
            var specMessage = string.Join(
                Environment.NewLine, string.Empty, _builder, "----", assignmentList);
            throw new XunitException(message, new XunitException(specMessage));
        }
    }

    public void AddThen() => _builder.Add(_builder.AddThen);

    internal void AddBecause(string reason) => _builder.SetBecause(reason);

    public void AddVerify<TService>(string expressionExpr)
        => _builder.Add(() => _builder.AddVerify<TService>(expressionExpr));

    public void AddAssertThrows<TError>(string? binder = null)
        => _builder.Add(() => _builder.AddAssertThrows<TError>(binder));

    public void AddAssertThrows(string expectedExpr)
        => _builder.Add(() => _builder.AddAssertThrows(expectedExpr));

    public void AddAssert([CallerMemberName] string? assertName = null)
         => _builder.Add(() => _builder.AddAssert(assertName!));

    public void AddAssertConjunction(string conjunction)
         => _builder.Add(() => _builder.AddAssertConjunction(conjunction));

    public void AddThat() => _builder.Add(_builder.AddThat);

    // ----------- Lifecycle

    internal static SpecificationContext Create() => _currentAssertionContext = new();

    internal static void Release() => _currentAssertionContext = null;

    private static XunitException? GetExpectedException(XunitException? ex)
        => ex is null || ex.Message.StartsWith("Expected")
            ? ex
            : GetExpectedException(ex.InnerException as XunitException);
}