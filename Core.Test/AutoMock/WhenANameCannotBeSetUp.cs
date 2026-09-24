using TSpec.Assert;

namespace TSpec.Test.AutoMock;

/// Each of these names a member the test cannot mean, so the failure says which limit was met.
public class WhenANameCannotBeSetUp : Spec<CatalogService, string>
{
    private string Refusal(Action arrange)
        => Xunit.Assert.Throws<SetupFailed>(() =>
        {
            arrange();
            When(_ => _.CountOf(A<string>())).Then().Completes();
        }).Message;

    [Fact]
    public void GivenANameOfNoMember_ThenSaySo()
        => Refusal(() => Given<ICatalog>().That<int>("Cuont").Returns(() => 7))
            .Is("ICatalog has no method or property named 'Cuont'. "
                + "Check the spelling, and prefer nameof so the compiler checks it");

    [Fact]
    public void GivenATypeNoMemberOfTheNameCanReturn_ThenSayWhatTheyReturn()
        => Refusal(() => Given<ICatalog>().That<string>(nameof(ICatalog.Count)).Returns(() => "seven"))
            .Is("ICatalog.Count does not return string. It returns int. "
                + "Name the type the member answers with — for an async member, the value inside the task");

    [Fact]
    public void GivenNothingToAnswerWithOfAMemberThatAnswersWithSomething_ThenSayWhatItReturns()
        => Refusal(() => Given<ICatalog>().That(nameof(ICatalog.Count)).Returns())
            .Is("ICatalog.Count does not return void or Task or ValueTask. It returns int. "
                + "Name the type the member answers with — for an async member, the value inside the task");

    [Fact]
    public void GivenANameOfNoMemberThatCanBeIntercepted_ThenSaySo()
        => Refusal(() => Given<Shelf>().That<string>(nameof(Shelf.Fixed)).Returns(() => "named"))
            .Is("Shelf.Fixed is not virtual or abstract, so nothing can intercept it. "
                + "Only a member the mock can override may be set up");

    [Fact]
    public void GivenATapReadingTheArgumentsOfSeveralMembers_ThenSayItCannotReadThem()
        => Refusal(() => Given<ICatalog>().That<string>(nameof(ICatalog.Find)).Tap<int>(_ => { }).Returns(() => "found"))
            .Is("ICatalog.Find names several members, so a Tap or Returns cannot read their arguments. "
                + "Set up the one you mean with an expression: That(_ => _.Find(…))");

    [Fact]
    public void GivenAnAnswerFromTheArgumentsOfSeveralMembers_ThenSayItCannotReadThem()
        => Refusal(() => Given<ICatalog>().That<string>(nameof(ICatalog.Find)).Returns((int id) => $"{id}"))
            .Is("ICatalog.Find names several members, so a Tap or Returns cannot read their arguments. "
                + "Set up the one you mean with an expression: That(_ => _.Find(…))");

    [Fact]
    public void GivenATapReadingTheArgumentsOfOneMember_ThenItIsSetUp()
        => When(_ => _.ReadText(A<string>()))
            .Given<ICatalog>().That<string>(nameof(ICatalog.Read)).Tap<string>(_ => { }).Returns(() => "text")
            .Then().Result.Is("text");
}
