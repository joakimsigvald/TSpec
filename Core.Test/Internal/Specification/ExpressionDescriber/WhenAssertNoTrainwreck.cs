using TSpec.Assert;
using TSpec.Internal.Specification;

namespace TSpec.Test.Internal.Specification.ExpressionDescriber;

public class WhenAssertNoTrainwreck : Spec
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("Result")]
    [InlineData("The<Checkout>()")]
    [InlineData("The<Checkout>().Basket")]
    [InlineData("Three<MyModel>().Last()")]
    [InlineData("A<MyObject>(_ => _.Age = 3)")]
    public void GivenSimpleSubject_ThenDoNotThrow(string? expr)
        => expr.AssertNoTrainwreck();

    [Theory]
    [InlineData("Result.Length")]
    [InlineData("value.Property")]
    [InlineData("Property1.Property2.Property3")]
    [InlineData("a.b().c")]
    public void GivenTrainwreck_ThenThrow(string expr)
        => Xunit.Assert.Throws<SetupFailed>(() => expr.AssertNoTrainwreck());

    [Theory]
    [InlineData("Result.Length", "Then(Result).Length")]
    [InlineData("Property1.Property2.Property3", "Then(Property1).Property2.Property3")]
    [InlineData("a.b().c", "Then(a).b().c")]
    public void GivenTrainwreck_ThenPointAtTheIdiom(string expr, string idiom)
        => Xunit.Assert.Throws<SetupFailed>(() => expr.AssertNoTrainwreck(verb: "Then")).Message.Is(
            $"No trainwrecks in Then: '{expr}' chains a member on its subject. "
            + $"Hand over the root and chain the rest after it: {idiom}");
}
