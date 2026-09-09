using TSpec.Assert;

namespace TSpec.Test.Pipeline;

/// <summary>
/// Arrangement is deferred: what a Given provides lands when the pipeline runs, not where it is
/// written. So a value read before that point yields the generated placeholder, and a test comparing
/// against it passes by luck of evaluation order. The read is only wrong once an arrangement lands on
/// the same value, which is what TSpec detects — a read of something nothing arranges is left alone.
/// </summary>
public class WhenAValueIsReadBeforeItIsArranged
{
    private sealed class MySpec : Spec<int>
    {
        internal string ReadName() => The(_name);
        internal string ReadFirstString() => The<string>();
        internal string[] ReadTwoStrings() => Two<string>();

        internal string[] ArrangeTwoReadStrings()
        {
            var read = Two<string>();
            Given(read);
            return read;
        }
        internal static readonly Tag<string> _name = new(nameof(_name));
        internal static readonly Tag<string> _other = new(nameof(_other));
    }

    private sealed class CartSpec : Spec<CartService, int>
    {
        internal Cart ReadCart() => The<Cart>();
    }

    public sealed class Cart
    {
        public List<string> Items { get; } = [];
    }

    public sealed class CartService
    {
        public int Add(Cart cart, string item)
        {
            cart.Items.Add(item);
            return cart.Items.Count;
        }
    }

    private const string ReadTooEarly =
        "the tag '_name' was read before the pipeline was arranged, so the read yielded a generated "
        + "value rather than the one arranged for it. Run the pipeline with Then() before reading it";

    [Fact]
    public void GivenTagReadBeforeTheRun_ThenThrowSetupFailed()
    {
        using var spec = new MySpec();
        spec.Given(MySpec._name).Is("given").When(() => 1);
        spec.ReadName();
        Xunit.Assert.Throws<SetupFailed>(() => spec.Then().Result.Is(1)).Message.Is(ReadTooEarly);
    }

    [Fact]
    public void GivenTagReadAsTheThenSubject_ThenThrowSetupFailed()
    {
        using var spec = new MySpec();
        spec.Given(MySpec._name).Is("given").When(() => 1);
        var name = spec.ReadName();
        Xunit.Assert.Throws<SetupFailed>(() => spec.Then(name).Is("given")).Message.Is(ReadTooEarly);
    }

    [Fact]
    public void GivenNumberedMentionReadBeforeTheRun_ThenThrowSetupFailed()
    {
        using var spec = new MySpec();
        spec.Given().A("given").When(() => 1);
        spec.ReadFirstString();
        Xunit.Assert.Throws<SetupFailed>(() => spec.Then().Result.Is(1)).Message.Is(
            "the first string was read before the pipeline was arranged, so the read yielded a generated "
            + "value rather than the one arranged for it. Run the pipeline with Then() before reading it");
    }

    /// An element of a collection is a numbered slot like any other, so an early read of one counts.
    [Fact]
    public void GivenCollectionReadBeforeTheRun_ThenThrowSetupFailedWhenAnElementIsArranged()
    {
        using var spec = new MySpec();
        spec.Given().A("given").When(() => 1);
        spec.ReadTwoStrings();
        Xunit.Assert.Throws<SetupFailed>(() => spec.Then().Result.Is(1)).Message.Is(
            "the first string was read before the pipeline was arranged, so the read yielded a generated "
            + "value rather than the one arranged for it. Run the pipeline with Then() before reading it");
    }

    /// <summary>
    /// The read feeds the arrangement, which stores the very values it was handed, so nothing the test
    /// holds goes stale. This is the idiom for "generate some values and use them as the given ones".
    /// </summary>
    [Fact]
    public void GivenTheReadValuesAreThemselvesWhatIsArranged_ThenDoNotComplain()
    {
        using var spec = new MySpec();
        var given = spec.ArrangeTwoReadStrings();
        spec.When(() => 1).Then().Result.Is(1);
        spec.ReadTwoStrings().Is().EqualTo(given);
    }

    [Fact]
    public void GivenCollectionReadEarlyButNothingArrangesIt_ThenDoNotComplain()
    {
        using var spec = new MySpec();
        spec.When(() => 1);
        var read = spec.ReadTwoStrings();
        spec.Then().Result.Is(1);
        spec.ReadTwoStrings().Is().EqualTo(read);
    }

    [Fact]
    public void GivenTagReadAfterTheRun_ThenDoNotComplain()
    {
        using var spec = new MySpec();
        spec.Given(MySpec._name).Is("given").When(() => 1).Then().Result.Is(1);
        spec.ReadName().Is("given");
    }

    [Fact]
    public void GivenTagReadOnlyInsideTheAct_ThenDoNotComplain()
    {
        using var spec = new MySpec();
        spec.Given(MySpec._name).Is("given")
            .When(() => spec.ReadName().Length)
            .Then().Result.Is(5);
    }

    [Fact]
    public void GivenTagReadEarlyButNothingArrangesIt_ThenDoNotComplain()
    {
        using var spec = new MySpec();
        spec.Given(MySpec._other).Is("given").When(() => 1);
        var read = spec.ReadName();
        spec.Then().Result.Is(1);
        spec.ReadName().Is(read);
    }

    /// <summary>
    /// An arrangement that mutates rather than replaces leaves the read reference pointing at the very
    /// object the pipeline uses, so the test is holding the right thing and nothing is stale.
    /// </summary>
    [Fact]
    public void GivenTheReadValueIsMutatedInPlaceByAnArrangement_ThenDoNotComplain()
    {
        using var spec = new CartSpec();
        var cart = spec.ReadCart();
        spec.Given().A<Cart>(c => c.Items.Add("pear"))
            .When(_ => _.Add(spec.ReadCart(), "apple"))
            .Then().Result.Is(2);
        cart.Items.Is().EqualTo(["pear", "apple"]);
    }

    /// <summary>
    /// The read is early, but nothing arranges the cart, so the value read is the very object the act
    /// mutates — the idiom for stating the effect of a void act on its input.
    /// </summary>
    [Fact]
    public void GivenInputReadEarlyAndMutatedByTheAct_ThenDoNotComplain()
    {
        using var spec = new CartSpec();
        var cart = spec.ReadCart();
        spec.When(_ => _.Add(spec.ReadCart(), "apple"));
        spec.Then(cart).Items.Is().EqualTo(["apple"]);
    }
}
