namespace TSpec.Analyzers.Test;

/// Every spec inherits a class fixture no constructor takes, which xUnit hints at on every spec class.
public class WhenASpecTakesNoFixture
{
    private const string Spec = """
        public class WhenAddingOne : Spec<int>
        {
            public WhenAddingOne() => When(() => 1 + 1);

            [Fact] public void ThenItIsTwo() => Result.Is(2);
        }
        """;

    private const string SpecWithAFixtureOfItsOwn = """
        public sealed class Database : IDisposable
        {
            public void Dispose() { }
        }

        public class WhenAddingOne : Spec<int>, IClassFixture<Database>
        {
            public WhenAddingOne() => When(() => 1 + 1);

            [Fact] public void ThenItIsTwo() => Result.Is(2);
        }
        """;

    [Fact]
    public async Task GivenNothingSuppressesIt_ThenXunitHintsAtSharedRunScope()
        => (await FixtureHints.Shown(Spec)).Single().Does().Contain("SharedRunScope");

    [Fact]
    public async Task ThenHideTheHintAtSharedRunScope()
        => (await FixtureHints.Shown(Spec, new SharedRunScopeSuppressor())).Is().Empty();

    [Fact]
    public async Task GivenAFixtureOfItsOwn_ThenKeepTheHintAtThat()
        => (await FixtureHints.Shown(SpecWithAFixtureOfItsOwn, new SharedRunScopeSuppressor()))
            .Single().Does().Contain("Database");
}
