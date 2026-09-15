using TSpec.Assert;

namespace TSpec.Test.AutoMock;

public interface IQuote
{
    Task<string> GetAsync(int supplier);
}

public class QuoteRace(IQuote quote)
{
    public async Task<string> FirstOf(int supplier, int otherSupplier)
        => await await Task.WhenAny(quote.GetAsync(supplier), quote.GetAsync(otherSupplier));
}

/// Returns supplies the value inside the task, so a task that completes later is set up by stating the
/// task itself as the call's return type.
public class WhenAMockedTaskCompletesLater : Spec<QuoteRace, string>
{
    [Fact]
    public void ThenTheCallThatCompletedAnswersFirst()
        => Given<IQuote>().That<Task<string>>(_ => _.GetAsync(1)).Returns(() => new TaskCompletionSource<string>().Task)
            .AndThat(_ => _.GetAsync(2)).Returns(() => "second")
            .When(_ => _.FirstOf(1, 2))
            .Then().Result.Is("second");
}
