namespace TSpec.Test.AutoMock;

public abstract class Awkward
{
    protected abstract string Over(int id);
    protected abstract string Over(int id, string tag);
    protected abstract bool TryGet(int id, out string found);
    protected virtual T Echo<T>(T value) => value;
    protected abstract string Label { get; }
}

public class AwkwardService(Awkward awkward)
{
    public string Go() => awkward.ToString()!;
}

/// A name reaches a protected member as it reaches any other, so what ThatProtected refused is set up.
public class WhenAProtectedMemberIsNamed : Spec<AwkwardService, string>
{
    public WhenAProtectedMemberIsNamed() => When(_ => _.Go());

    [Fact]
    public void GivenOverloads_ThenTheyAreSetUp()
    {
        Given<Awkward>().That<string>("Over").Returns(() => "x");
        Then().Completes();
    }

    [Fact]
    public void GivenAnOutParameter_ThenItIsSetUp()
    {
        Given<Awkward>().That<bool>("TryGet").Returns(() => true);
        Then().Completes();
    }

    [Fact]
    public void GivenAGenericMember_ThenItIsSetUp()
    {
        Given<Awkward>().That<string>("Echo").Returns(() => "x");
        Then().Completes();
    }

    [Fact]
    public void GivenAProperty_ThenItIsSetUp()
    {
        Given<Awkward>().That<string>("Label").Returns(() => "labelled");
        Then().Completes();
    }

    [Fact]
    public void GivenThatProtected_ThenItSetsUpAsThatDoes()
    {
#pragma warning disable CS0618 // the obsolete form, kept working until it is removed
        Given<Awkward>().ThatProtected<string>("Over").Returns(() => "x");
#pragma warning restore CS0618
        Then().Completes();
    }
}
