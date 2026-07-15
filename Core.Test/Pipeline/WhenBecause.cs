using TSpec.Assert;
using Xunit.Sdk;

namespace TSpec.Test.Pipeline;

public class WhenBecause : Spec<MyStateService, int>
{
    [Fact]
    public void ThenReasonIsAppendedToSpecification()
    {
        When(_ => ++_.Counter).Then(because: "incrementing zero yields one").Result.Is(1);
        Specification.Is(
            """
            When ++_.Counter
            Then Result is 1, because incrementing zero yields one
            """);
    }

    [Fact]
    public void SyntacticSugarForThenBecause()
    {
        When(_ => ++_.Counter);
        Because("incrementing zero yields one").Result.Is(1);
        Specification.Is(
            """
            When ++_.Counter
            Then Result is 1, because incrementing zero yields one
            """);
    }

    [Fact]
    public void GivenChainedAssertions_ThenReasonIsAppendedAfterLastAssertion()
    {
        When(_ => ++_.Counter).Then(because: "it is one")
            .Result.Is().GreaterThan(0).and.LessThan(2);
        Specification.Does().EndWith("less than 2, because it is one");
    }

    [Fact]
    public void GivenThrows_ThenReasonIsAppendedToSpecification()
    {
        When(_ => ThrowIt()).Then(because: "the method always throws").Throws<InvalidOperationException>();
        Specification.Is(
            """
            When ThrowIt()
            Then throws InvalidOperationException, because the method always throws
            """);
    }

    [Fact]
    public void GivenAssertionFails_ThenReasonIsIncludedInFailureOutput()
    {
        var ex = Xunit.Assert.Throws<XunitException>(
            () => When(_ => ++_.Counter).Then(because: "of a flawed expectation").Result.Is(2));
        ex.InnerException!.Message.Does().Contain(", because of a flawed expectation");
    }

    private static int ThrowIt() => throw new InvalidOperationException();
}