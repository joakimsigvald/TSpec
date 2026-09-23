using System.Linq.Expressions;

namespace TSpec.Internal.TestData.Generation.Strategies.Mocking;

/// Every mock of a type, or one mock: what a setup is made on, and what a verification counts.
internal interface IMocked
{
    string Name { get; }

    CallSetups Setups { get; }

    IReadOnlyList<MockInvocation> CountedInvocations { get; }

    int CountCalls(LambdaExpression call, string callExpr);
}
