using TSpec.Internal.Specification;

namespace TSpec.Assert.Continuations;

internal record DoesValue<TValue> : Constraint<TValue, DoesValue<TValue>>
{
    internal ContinueWithActual<TValue> Have(
        Func<TValue, bool> condition, string? conditionExpr = null)
    {
        Assert(conditionExpr!.ParseValue(), NotNullAnd(actual => Xunit.Assert.True(condition(actual))), conditionExpr!, "", verbalizationStrategy: VerbalizationStrategy.PresentSingularS);
        return new(Actual);
    }
}