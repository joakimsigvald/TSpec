using System.Runtime.CompilerServices;

namespace TSpec.Internal.Specification;

internal interface IAssertSpecificationContext 
{
    void Assert(Action assert, string actual, string? expected, string verb);
    void AddThen();
    void SetSubject(string subjectExpr, [CallerMemberName] string? provider = null);
    void ClearSubject();
    void AddVerify<TService>(string mock, string expressionExpr, string? wasInvokedExpr);
    void AddWasInvoked(string mock, string? wasInvokedExpr);
    void AddWasInvoked(string mock, string method, string? wasInvokedExpr);
    void AddAssertThrows<TError>(string? binder = null);
    void AddAssertThrows(string expectedExpr);
    void AddAssert([CallerMemberName] string? assertName = null);
    void AddAssertConjunction(string conjunction);
    void AddThat();
    void AddSetupWarning(string warning);
}