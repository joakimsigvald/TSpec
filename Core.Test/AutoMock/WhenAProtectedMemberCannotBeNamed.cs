using TSpec.Assert;

namespace TSpec.Test.AutoMock;

public abstract class Awkward
{
    protected abstract string Over(int id);
    protected abstract string Over(int id, string tag);
    protected abstract bool TryGet(int id, out string found);
    protected virtual T Echo<T>(T value) => value;
    protected string NotVirtual(int id) => "base";
    protected abstract int Counted(int id);
    public virtual string Open(int id) => "open";
    protected abstract string Label { get; }
}

public class AwkwardService(Awkward awkward)
{
    public string Go() => awkward.ToString()!;
}

/// <summary>
/// Naming a member says nothing about its arguments, and Moq can only intercept what the mock can
/// override. Each of these is a member the test named correctly, so none may be reported as one
/// that does not exist — the failure has to say which limit was met.
/// </summary>
public class WhenAProtectedMemberCannotBeNamed : Spec<AwkwardService, string>
{
    private string Refusal(Action arrange)
        => Xunit.Assert.Throws<SetupFailed>(() =>
        {
            arrange();
            When(_ => _.Go()).Then().Completes();
        }).Message;

    [Fact]
    public void GivenOverloads_ThenSayANameCannotChooseBetweenThem()
        => Refusal(() => Given<Awkward>().ThatProtected<string>("Over").Returns(() => "x"))
            .Does().Contain("is overloaded").and.Contain("cannot say which one is meant");

    [Fact]
    public void GivenAnOutParameter_ThenSayItCannotBeMatched()
        => Refusal(() => Given<Awkward>().ThatProtected<bool>("TryGet").Returns(() => true))
            .Does().Contain("takes 'found' by reference");

    [Fact]
    public void GivenAGenericMember_ThenSayANameCarriesNoTypeArgument()
        => Refusal(() => Given<Awkward>().ThatProtected<string>("Echo").Returns(() => "x"))
            .Does().Contain("is generic, and a name carries no type argument");

    [Fact]
    public void GivenANonVirtualMember_ThenSayNothingCanInterceptIt()
        => Refusal(() => Given<Awkward>().ThatProtected<string>("NotVirtual").Returns(() => "x"))
            .Does().Contain("is not virtual or abstract, so nothing can intercept it");

    [Fact]
    public void GivenAPublicMember_ThenPointAtTheExpressionForm()
        => Refusal(() => Given<Awkward>().ThatProtected<string>("Open").Returns(() => "x"))
            .Does().Contain("is public, so state the call as an expression");

    [Fact]
    public void GivenTheWrongReturnType_ThenSayWhatItReturns()
        => Refusal(() => Given<Awkward>().ThatProtected<string>("Counted").Returns(() => "x"))
            .Does().Contain("does not return string").and.Contain("It returns int");

    [Fact]
    public void GivenNoSuchMember_ThenSaySo()
        => Refusal(() => Given<Awkward>().ThatProtected<string>("Nonesuch").Returns(() => "x"))
            .Does().Contain("has no member named 'Nonesuch'");

    /// A protected property is a protected member, and Moq reaches one by name too.
    [Fact]
    public void GivenAProperty_ThenSetItUp()
    {
        Given<Awkward>().ThatProtected<string>("Label").Returns(() => "labelled");
        When(_ => _.Go()).Then().Completes();
    }
}
