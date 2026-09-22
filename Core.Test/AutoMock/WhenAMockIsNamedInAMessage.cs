using TSpec.Assert;
using TSpec.Internal.Specification;
using Xunit.Sdk;

namespace TSpec.Test.AutoMock;

public interface IChildSink
{
    void Accept(IChild child);
    void AcceptGrandChild(IGrandChild grandChild);
}

public class ChildSinkService(IParent parent, IChildSink sink)
{
    public string PassOnChildOf(int id)
    {
        sink.Accept(parent.GetChild(id));
        return string.Empty;
    }

    public string PassOnTheChild()
    {
        sink.Accept(parent.Child);
        return string.Empty;
    }

    public string PassOnGrandChildOf(int id)
    {
        sink.AcceptGrandChild(parent.GetChild(id).GrandChild);
        return string.Empty;
    }
}

public class ChildFactorySinkService(Func<int, IChild> childOf, IChildSink sink)
{
    public string PassOnChildOf(int id)
    {
        sink.Accept(childOf(id));
        return string.Empty;
    }
}

/// A mock names itself by where it came from: the shared mock by its type, a child by the call that made it.
public class WhenAMockIsNamedInAMessage : Spec<ParentService, IChild>
{
    [Fact]
    public void GivenTheSharedMockIsExpected_ThenItIsNamedByItsType()
        => MessageOf(() => When(_ => _.ChildOf(1)).Then().Result.Is(The<IChild>()))
            .Is("Expected Result to be the IChild but found IChild from IParent.GetChild(1)");

    [Fact]
    public void GivenAnotherChildIsExpected_ThenBothAreNamedByTheCallThatMadeThem()
        => MessageOf(() => When(_ => _.ChildOf(1)).Then().Result.Is(The<IParent>().GetChild(2)))
            .Is("Expected Result to be IChild from IParent.GetChild(2) "
                + "but found IChild from IParent.GetChild(1)");

    internal static string MessageOf(Action assertion)
        => Xunit.Assert.Throws<XunitException>(assertion).Message.NormalizeLineEndings();
}

public class WhenAMockIsListedAsAnArgument : Spec<ChildSinkService, string>
{
    [Fact]
    public void GivenTheChildWasPassedOn_ThenTheListingNamesTheCallThatMadeIt()
        => MessageOf(() => When(_ => _.PassOnChildOf(1)).Then<IChildSink>(_ => _.Accept(The<IChild>())))
            .Is(Expected(
                """
                Expected IChildSink.Accept(the IChild) to be invoked at least once but was never invoked
                IChildSink received:
                  IChildSink.Accept(IChild from IParent.GetChild(1))
                """));

    [Fact]
    public void GivenAPropertyMadeTheChild_ThenTheListingNamesTheProperty()
        => MessageOf(() => When(_ => _.PassOnTheChild()).Then<IChildSink>(_ => _.Accept(The<IChild>())))
            .Is(Expected(
                """
                Expected IChildSink.Accept(the IChild) to be invoked at least once but was never invoked
                IChildSink received:
                  IChildSink.Accept(IChild from IParent.Child)
                """));

    [Fact]
    public void GivenAGrandChildWasPassedOn_ThenItsNameComposesTheWholeChain()
        => MessageOf(() => When(_ => _.PassOnGrandChildOf(1))
                .Then<IChildSink>(_ => _.AcceptGrandChild(The<IGrandChild>())))
            .Is(Expected(
                """
                Expected IChildSink.AcceptGrandChild(the IGrandChild) to be invoked at least once but was never invoked
                IChildSink received:
                  IChildSink.AcceptGrandChild(IGrandChild from IParent.GetChild(1).GrandChild)
                """));

    private static string MessageOf(Action verification) => WhenAMockIsNamedInAMessage.MessageOf(verification);

    private static string Expected(string message) => message.NormalizeLineEndings();
}

public class WhenADelegateMadeTheChild : Spec<ChildFactorySinkService, string>
{
    [Fact]
    public void ThenTheChildIsNamedByTheDelegateCall()
        => MessageOf(() => When(_ => _.PassOnChildOf(1)).Then<IChildSink>(_ => _.Accept(The<IChild>())))
            .Is(Expected(
                """
                Expected IChildSink.Accept(the IChild) to be invoked at least once but was never invoked
                IChildSink received:
                  IChildSink.Accept(IChild from Func<int, IChild>(1))
                """));

    private static string MessageOf(Action verification) => WhenAMockIsNamedInAMessage.MessageOf(verification);

    private static string Expected(string message) => message.NormalizeLineEndings();
}
