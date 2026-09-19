using TSpec.Assert;
using TSpec.Test.AutoMock;

namespace TSpec.Test.Pipeline;

/// A pipeline runs once, so running it again after arranging failed is refused rather than half-done.
public class WhenThePipelineRunsAgainAfterFailing : Spec<SealedClientService, string>
{
    [Fact]
    public void ThenItIsRefusedSayingWhy()
    {
        When(_ => _.Fetch()).Given<SealedClient>().That(_ => _.Fetch()).Returns(() => "mocked");
        Xunit.Assert.Throws<SetupFailed>(() => Then());
        Xunit.Assert.Throws<InvalidOperationException>(() => Then()).Message.Is(
            "Cannot advance the pipeline from Arrange to Arrange. A pipeline runs once, so do not run it "
            + "again after it failed; if the test does not, this is a bug in TSpec. "
            + "Please report it at https://github.com/joakimsigvald/TSpec/issues");
    }
}
