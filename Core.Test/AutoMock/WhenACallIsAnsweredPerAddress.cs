using TSpec.Assert;
using static TSpec.Times;

namespace TSpec.Test.AutoMock;

/// A mock answers one address — a method and its arguments — with one thing, however often it is called.
public class WhenACallIsAnsweredPerAddress : Spec<ParentService, IChild>
{
    public WhenACallIsAnsweredPerAddress() => When(_ => _.ChildOf(1));

    [Fact]
    public void GivenTheSameCallTwice_ThenItAnswersTheSame()
        => Result.Get(9).Is(Result.Get(9));

    [Fact]
    public void GivenAnotherArgument_ThenItAnswersAnother()
        => Result.Get(9).Is().Not(Result.Get(8));

    [Fact]
    public void GivenTheSameAddressTwice_ThenItIsTheSameChild()
        => ReferenceEquals(The<IParent>().GetChild(1), The<IParent>().GetChild(1)).Is(true);

    [Fact]
    public void GivenAnotherAddress_ThenItIsAnotherChild()
        => ReferenceEquals(The<IParent>().GetChild(1), The<IParent>().GetChild(2)).Is(false);

    [Fact]
    public async Task GivenTheCallIsAwaited_ThenAnotherAddressIsAnotherChildToo()
        => ReferenceEquals(await The<IParent>().GetChildAsync(1), await The<IParent>().GetChildAsync(2)).Is(false);
}

/// A chain counts the calls made at the address it names, not on every mock of the type.
public class WhenAChainIsVerifiedThroughAnUnmatchedStep : Spec<ParentService, string>
{
    [Fact]
    public void ThenEachAddressCountsItsOwn()
        => When(_ => _.GetFromChildrenOf(1, 2, 9))
            .Then<IParent>(_ => _.GetChild(1).Get(9), Once)
            .And<IParent>(_ => _.GetChild(2).Get(9), Once);
}
