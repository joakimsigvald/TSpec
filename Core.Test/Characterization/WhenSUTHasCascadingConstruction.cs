using TSpec.Assert;

namespace TSpec.Test.Characterization;

public class WhenSUTHasMultiLevelConcreteDeps : Spec<CascadeRoot, CascadeLeaf>
{
    public WhenSUTHasMultiLevelConcreteDeps() => When(_ => _.GetLeaf());

    [Fact]
    public void Then_AllLevelsAreConstructed_AndLeafIsNonNull()
    {
        Then().Result.Is().Not(null!);
        Specification.Is(
            """
            When _.GetLeaf()
            Then Result is not null
            """);
    }
}

public class WhenSUTSharesMockedInterfaceAcrossDeps : Spec<MockShareSUT, bool>
{
    public WhenSUTSharesMockedInterfaceAcrossDeps() => When(_ => _.DirectAndIndirectShareSameInstance());

    [Fact]
    public void Then_SameMockInstanceIsInjectedToBothPaths()
    {
        Then().Result.Is(true);
        Specification.Is(
            """
            When _.DirectAndIndirectShareSameInstance()
            Then Result is true
            """);
    }
}

public class CascadeLeaf
{
    public int Value { get; set; }
}

public class CascadeMid(CascadeLeaf leaf)
{
    private readonly CascadeLeaf _leaf = leaf;
    public CascadeLeaf Leaf => _leaf;
}

public class CascadeRoot(CascadeMid mid)
{
    private readonly CascadeMid _mid = mid;
    public CascadeLeaf GetLeaf() => _mid.Leaf;
}

public interface ISharedDep
{
    int Get();
}

public class IndirectHolder(ISharedDep dep)
{
    public ISharedDep Dep { get; } = dep;
}

public class MockShareSUT(ISharedDep direct, IndirectHolder indirect)
{
    private readonly ISharedDep _direct = direct;
    private readonly IndirectHolder _indirect = indirect;
    public bool DirectAndIndirectShareSameInstance()
        => ReferenceEquals(_direct, _indirect.Dep);
}
