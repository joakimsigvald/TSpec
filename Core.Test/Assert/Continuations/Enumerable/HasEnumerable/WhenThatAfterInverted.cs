using TSpec.Assert;

namespace TSpec.Test.Assert.Continuations.Enumerable.HasEnumerable;

public class WhenThatAfterInverted : Spec
{
    [Fact]
    public void GivenNotOneItem()
    {
        int[] numbers = [1, 2];
        var continuation = numbers.Has().not.OneItem();
        var ex = Xunit.Assert.Throws<SetupFailed>(() => { _ = continuation.that; });
        ex.Message.Is("Cannot access 'that' after an inverted assertion");
    }

    [Fact]
    public void GivenNotTwoItems()
    {
        int[] numbers = [1];
        var continuation = numbers.Has().not.TwoItems();
        Xunit.Assert.Throws<SetupFailed>(() => { _ = continuation.that; });
    }
}
