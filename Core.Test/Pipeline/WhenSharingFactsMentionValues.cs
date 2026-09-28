using TSpec.Assert;

namespace TSpec.Test.Pipeline;

/// Each Fact mentions the act's values in the reverse of the order the act made them, so a Fact
/// that generated its own values would get them the other way round.
public class WhenSharingFactsMentionValues : Spec<string>
{
    public WhenSharingFactsMentionValues() => When(() => $"{A<string>()}/{ASecond<string>()}");

    [Fact] public void ThenOneSeesTheRunsValues() => ThenTheValuesAreTheRuns();

    [Fact] public void ThenAnotherSeesThemToo() => ThenTheValuesAreTheRuns();

    private void ThenTheValuesAreTheRuns()
        => Result.Does().EndWith(TheSecond<string>()).and.StartWith(The<string>());
}
