using System.Runtime.CompilerServices;

namespace TSpec.Internal.Specification;

internal interface IAssertSpecificationContext 
{
    void Assert(Action assert, string actual, string? expected, string verb);
    void AddThen();
    void SetSubject(string? subjectExpr);
    void AddVerify<TService>(string expressionExpr);
    void AddWasInvoked<TService>(string? timesExpr);
    void AddWasInvoked<TService>(string method, string? timesExpr);
    void AddAssertThrows<TError>(string? binder = null);
    void AddAssertThrows(string expectedExpr);
    void AddAssertDoesNotThrow<TError>();
    void AddAssert([CallerMemberName] string? assertName = null);
    void AddAssertConjunction(string conjunction);
    void AddThat();
}