using TSpec.Internal.Specification;

namespace TSpec.Test.Internal.Specification.ExpressionParser;

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
}
