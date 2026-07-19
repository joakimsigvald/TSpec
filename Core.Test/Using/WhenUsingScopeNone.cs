using TSpec.Assert;

namespace TSpec.Test.Using;

public class WhenUsingScopeNone : Spec<int>
{
    [Fact]
    public void ThenSetupFailedExplainsScope()
    {
        var ex = Xunit.Assert.Throws<SetupFailed>(() => Using(42, For.None));
        ex.Message.Is("Unsupported scope: None");
    }
}
