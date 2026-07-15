using TSpec.Assert;

namespace TSpec.Test.Using;

public class WhenUsingChainedDerivation : Spec<WhenUsingChainedDerivation.ChainSut, bool>
{
    [Fact]
    public void ThenDerivedValuesReferenceTheRegisteredRoot()
    {
        Using(new Root())
            .And(The<Root>().CreateMiddle())
            .And(The<Middle>().CreateLeaf())
            .When(_ => _.AllConnected(The<Root>(), The<Middle>(), The<Leaf>()))
            .Then().Result.Is(true);
    }

    public class Root
    {
        public Middle CreateMiddle() => new(this);
    }

    public class Middle(Root parent)
    {
        public Root Parent { get; } = parent;
        public Leaf CreateLeaf() => new(this);
    }

    public class Leaf(Middle parent)
    {
        public Middle Parent { get; } = parent;
    }

    public class ChainSut
    {
        public bool AllConnected(Root root, Middle middle, Leaf leaf)
            => ReferenceEquals(root, middle.Parent)
            && ReferenceEquals(middle, leaf.Parent);
    }
}
