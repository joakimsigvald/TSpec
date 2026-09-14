using System.Runtime.CompilerServices;
using TSpec.Assert;
using static TSpec.Times;

[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]

namespace TSpec.Test.AutoMock;

public interface IMemberKinds
{
    int Sum(int[] values);
    string Name { get; set; }
    event EventHandler Changed;
    T Echo<T>(T value);
    bool TryGet(int id, out string value);
    string Get(int id);
    string Get(string key);
}

internal interface IInternalLookup
{
    string Get(int id);
}

public abstract class PartlyVirtual
{
    public virtual string Virtual() => "base";
    public string NonVirtual() => "real";
}

public delegate bool TryLookup(int id, out string value);

public class MemberKindsService(
    IMemberKinds kinds, PartlyVirtual partlyVirtual, Func<int, string> lookup, TryLookup tryLookup)
{
    public string TouchObjectMembers()
    {
        _ = kinds.GetHashCode();
        _ = kinds.Equals(kinds);
        return kinds.ToString()!;
    }

    public string SubscribeAndUnsubscribe()
    {
        kinds.Changed += OnChanged;
        kinds.Changed -= OnChanged;
        return string.Empty;
    }

    public string SumOf(int a, int b) => kinds.Sum([a, b]).ToString();
    public string SetName(string name) => kinds.Name = name;
    public string GetName() => kinds.Name;
    public string EchoInt(int value) => kinds.Echo(value).ToString();
    public string EchoString(string value) => kinds.Echo(value);
    public string TryGet(int id) => $"{kinds.TryGet(id, out var value)}:{value}";
    public string GetByKey(string key) => kinds.Get(key);
    public string GetById(int id) => kinds.Get(id);

    public string NameThenGet(string name)
    {
        kinds.Name = name;
        return kinds.Get(name);
    }

    public string CallVirtual() => partlyVirtual.Virtual();
    public string CallNonVirtual() => partlyVirtual.NonVirtual();
    public string Lookup(int id) => lookup(id);
    public string TryLookup(int id) => $"{tryLookup(id, out var value)}:{value}";

    private static void OnChanged(object? sender, EventArgs e) { }
}

/// What every object has is not a call on the mocked service; subscribing to its event is.
public class WhenAMockIsTouchedOutsideItsMembers : Spec<MemberKindsService, string>
{
    [Fact]
    public void GivenObjectMembers_ThenTheyAreNotInvocations()
        => When(_ => _.TouchObjectMembers()).Then<IMemberKinds>(wasInvoked: Never);

    [Fact]
    public void GivenAnEventSubscription_ThenEachAccessorIsAnInvocation()
        => When(_ => _.SubscribeAndUnsubscribe()).Then<IMemberKinds>(wasInvoked: Exactly(2));
}

/// A collection argument is matched by what it holds, not by which instance it is.
public class WhenAnArgumentIsACollection : Spec<MemberKindsService, string>
{
    public WhenAnArgumentIsACollection() => When(_ => _.SumOf(1, 2));

    [Fact]
    public void ThenASetupMatchesByContent()
        => Given<IMemberKinds>().That(_ => _.Sum(new[] { 1, 2 })).Returns(() => 3)
            .Then().Result.Is("3");

    [Fact]
    public void ThenAVerificationMatchesByContent()
        => Then<IMemberKinds>(_ => _.Sum(new[] { 1, 2 }));
}

public class WhenAPropertyIsMocked : Spec<MemberKindsService, string>
{
    [Fact]
    public void GivenItsGetterIsSetUp_ThenItAnswers()
        => When(_ => _.GetName())
            .Given<IMemberKinds>().That(_ => _.Name).Returns(() => "named")
            .Then().Result.Is("named");

    [Fact]
    public void GivenItIsSet_ThenTheSetterIsAnInvocation()
        => When(_ => _.SetName("x")).Then<IMemberKinds>(wasInvoked: Once);
}

/// A generic method is set up for the type argument it is written with, and no other.
public class WhenAGenericMethodIsMocked : Spec<MemberKindsService, string>
{
    [Fact]
    public void GivenTheSetUpTypeArgument_ThenItAnswers()
        => When(_ => _.EchoInt(5))
            .Given<IMemberKinds>().That(_ => _.Echo(5)).Returns(() => 7)
            .Then().Result.Is("7");

    [Fact]
    public void GivenAnotherTypeArgument_ThenTheSetupDoesNotAnswer()
        => When(_ => _.EchoString("5"))
            .Given<IMemberKinds>().That(_ => _.Echo(5)).Returns(() => 7)
            .Then().Result.Is().Not("7");
}

/// The setup matches whatever the out argument is, and hands back the value it was written with.
public class WhenAMethodHasAnOutParameter : Spec<MemberKindsService, string>
{
    private string _found = "found";

    [Fact]
    public void ThenTheSetupAnswersAndSetsIt()
        => When(_ => _.TryGet(1))
            .Given<IMemberKinds>().That(_ => _.TryGet(1, out _found)).Returns(() => true)
            .Then().Result.Is("True:found");
}

public class WhenAMethodIsOverloaded : Spec<MemberKindsService, string>
{
    [Fact]
    public void ThenASetupAnswersForItsOwnOverloadOnly()
        => When(_ => _.GetByKey("1"))
            .Given<IMemberKinds>().That(_ => _.Get(1)).Returns(() => "by id")
            .Then().Result.Is().Not("by id");
}

/// A setup's arguments are read as it is set up, so what they refer to may change afterwards.
public class WhenASetupArgumentChangesAfterArrangement : Spec<MemberKindsService, string>
{
    private int _id = 1;

    [Fact]
    public void ThenTheSetupKeepsTheValueItWasSetUpWith()
        => When(_ => _.GetById(2))
            .Given<IMemberKinds>().That(_ => _.Get(_id)).Returns(() => "matched")
            .Having(_ => _id = 2)
            .Then().Result.Is().Not("matched");
}

/// A mocked class answers for what it lets a mock override, and runs its own code for the rest.
public class WhenAnAbstractClassIsMocked : Spec<MemberKindsService, string>
{
    [Fact]
    public void GivenAVirtualMember_ThenItAnswersInsteadOfTheBase()
        => When(_ => _.CallVirtual()).Then().Result.Is().Not("base");

    [Fact]
    public void GivenANonVirtualMember_ThenItRunsItsOwnCode()
        => When(_ => _.CallNonVirtual()).Then().Result.Is("real");
}

public class WhenADelegateIsMocked : Spec<MemberKindsService, string>
{
    private string _found = "found";

    [Fact]
    public void ThenItsInvocationCanBeSetUp()
        => When(_ => _.Lookup(1))
            .Given<Func<int, string>>().That(_ => _(1)).Returns(() => "one")
            .Then().Result.Is("one");

    [Fact]
    public void GivenAnOutParameter_ThenTheSetupAnswersAndSetsIt()
        => When(_ => _.TryLookup(1))
            .Given<TryLookup>().That(_ => _(1, out _found)).Returns(() => true)
            .Then().Result.Is("True:found");
}

/// An internal interface is mocked once its assembly lets the proxies see it.
public class WhenAnInternalInterfaceIsMocked : Spec<object, string>
{
    [Fact]
    public void ThenItCanBeSetUp()
        => When(() => A<IInternalLookup>().Get(1))
            .Given<IInternalLookup>().That(_ => _.Get(1)).Returns(() => "internal")
            .Then().Result.Is("internal");
}

public class HttpService(HttpMessageHandler handler)
{
    public async Task<string> Send()
        => (await new HttpClient(handler).GetAsync("http://localhost/")).StatusCode.ToString();
}

public class WhenAnHttpMessageHandlerIsMocked : Spec<HttpService, string>
{
    [Fact]
    public void ThenItsProtectedSendCanBeSetUp()
        => When(_ => _.Send())
            .Given<HttpMessageHandler>().ThatProtected<HttpResponseMessage>("SendAsync")
            .Returns(() => new HttpResponseMessage(System.Net.HttpStatusCode.Accepted))
            .Then().Result.Is("Accepted");
}

/// A mock names itself as the specification names the service it stands in for.
public class WhenAMockIsRendered : Spec<MemberKindsService, string>
{
    [Fact]
    public void ThenItReadsAsTheMockedType()
        => When(_ => _.TouchObjectMembers()).Then().Result.Is("IMemberKinds");
}

public class WhenANonVirtualMemberIsSetUp : Spec<MemberKindsService, string>
{
    [Fact]
    public void ThenSetupFailsSayingWhy()
        => Xunit.Assert.Throws<SetupFailed>(() =>
            When(_ => _.CallNonVirtual())
                .Given<PartlyVirtual>().That(_ => _.NonVirtual()).Returns(() => "mocked")
                .Then())
            .Message.Is(
                "PartlyVirtual.NonVirtual is not virtual or abstract, so nothing can intercept it. "
                + "Only a member the mock can override may be set up or verified");
}
