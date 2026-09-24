using TSpec.Assert;

namespace TSpec.Test.AutoMock;

public class WhenANameReachesAMember : Spec<CatalogService, string>
{
    [Fact]
    public void GivenOneOfSeveralOverloads_ThenItIsAnswered()
        => When(_ => _.FindById(An<int>()))
            .Given<ICatalog>().That<string>(nameof(ICatalog.Find)).Returns(() => "found")
            .Then().Result.Is("found");

    [Fact]
    public void GivenAnotherOverload_ThenItIsAnsweredToo()
        => When(_ => _.FindByCode(A<string>()))
            .Given<ICatalog>().That<string>(nameof(ICatalog.Find)).Returns(() => "found")
            .Then().Result.Is("found");

    [Fact]
    public void GivenAnOverloadReturningABaseType_ThenItIsAnswered()
        => When(_ => _.FindByKey(A<Guid>()))
            .Given<ICatalog>().That<string>(nameof(ICatalog.Find)).Returns(() => "found")
            .Then().Result.Is("found");

    [Fact]
    public void GivenAnOverloadThatCannotReturnTheAnswer_ThenItIsLeftAlone()
        => When(_ => _.CountArchived())
            .Given<ICatalog>().Returns(() => 5)
            .AndThat<string>(nameof(ICatalog.Find)).Returns(() => "found")
            .Then().Result.Is("5");

    [Fact]
    public void GivenAnAsyncMember_ThenTheAnswerIsTheValueInsideTheTask()
        => When(_ => _.FindByIdAsync(An<int>()))
            .Given<ICatalog>().That<string>(nameof(ICatalog.FindAsync)).Returns(() => "found")
            .Then().Result.Is("found");

    [Fact]
    public void GivenAValueTaskMember_ThenTheAnswerIsTheValueInsideIt()
        => When(_ => _.FindSoon(An<int>()))
            .Given<ICatalog>().That<string>(nameof(ICatalog.FindSoonAsync)).Returns(() => "found")
            .Then().Result.Is("found");

    [Fact]
    public void GivenAProperty_ThenItsReadIsAnswered()
        => When(_ => _.ReadTitle())
            .Given<ICatalog>().That<string>(nameof(ICatalog.Title)).Returns(() => "named")
            .Then().Result.Is("named");

    [Fact]
    public void GivenAPropertyTheSubjectSets_ThenItReadsBackWhatWasSet()
        => When(_ => _.Retitle(A<string>()))
            .Given<ICatalog>().That<string>(nameof(ICatalog.Title)).Returns(() => "named")
            .Then().Result.Is(The<string>());

    [Fact]
    public void GivenAGenericMethod_ThenATypeArgumentThatCanHoldTheAnswerIsAnswered()
        => When(_ => _.ReadText(A<string>()))
            .Given<ICatalog>().That<string>(nameof(ICatalog.Read)).Returns(() => "text")
            .Then().Result.Is("text");

    [Fact]
    public void GivenAGenericMethodAtAnotherTypeArgument_ThenItIsLeftAlone()
        => When(_ => _.ReadNumber(A<string>()))
            .Given<ICatalog>().Returns(() => 5)
            .AndThat<string>(nameof(ICatalog.Read)).Returns(() => "text")
            .Then().Result.Is("5");

    [Fact]
    public void GivenAnOutParameter_ThenTheCallIsAnswered()
        => When(_ => _.TryFind(An<int>()))
            .Given<ICatalog>().That<bool>(nameof(ICatalog.TryFind)).Returns(() => true)
            .Then().Result.Is("True:");

    [Fact]
    public void GivenAProtectedMember_ThenItIsAnswered()
        => When(_ => _.StampOfRow(An<int>()))
            .Given<Shelf>().That<string>("Stamp").Returns(() => "stamped")
            .Then().Result.Is("stamped");

    [Fact]
    public void GivenANonVirtualOverload_ThenItIsLeftAlone()
        => When(_ => _.LabelOfName(A<string>()))
            .Given<Shelf>().That<string>(nameof(Shelf.Label)).Returns(() => "named")
            .Then().Result.Is("real");
}
