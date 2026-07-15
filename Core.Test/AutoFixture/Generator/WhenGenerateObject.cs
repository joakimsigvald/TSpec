using TSpec.Assert;

namespace TSpec.Test.AutoFixture.Generator;

public class WhenGenerateDeepModel : Spec<ModelLevel1>
{
    [Fact]
    public void ThenRootIsGenerated()
        => When(_ => _).Then().Result.Is().not.Null();

    [Fact]
    public void Then1stLevelSubComponentIsGenerated()
        => When(_ => _).Then().Result.Sub1.Is().not.Null();

    [Fact]
    public void Then2ndLevelSubComponentIsGenerated()
        => When(_ => _).Then().Result.Sub1.Sub2.Is().not.Null();

    [Fact]
    public void Then3rdLevelSubComponentIsGenerated()
        => When(_ => _).Then().Result.Sub1.Sub2.Sub3.Is().not.Null();
}

public class WhenGenerateWrappedSubComponent : Spec<WrappedModel>
{
    [Fact]
    public void ThenRootIsGenerated()
        => When(_ => _).Then().Result.Is().not.Null();

    [Fact]
    public void ThenWrappedSubComponentIsGenerated()
        => When(_ => _).Then().Result.LazySub.Value.Is().not.Null();

    [Fact]
    public void ThenDeepNestedSubComponentIsGenerated()
        => When(_ => _).Then().Result.LazySub.Value.Sub2.Sub3.Is().not.Null();
}

public class WhenGenerateSelfReferencingModel : Spec<SelfReferencingModel>
{
    [Fact]
    public void ThenRootIsGenerated()
        => When(_ => _).Then().Result.Is().not.Null();

    [Fact]
    public void ThenCycleTerminatesWithNull()
        => When(_ => _).Then().Result.Self.Is().Null();
}

public class ModelLevel4
{
}

public class ModelLevel3
{
    public ModelLevel4 Sub3 { get; set; } = null!;
}

public class ModelLevel2
{
    public ModelLevel3 Sub2 { get; set; } = null!;
}

public class ModelLevel1
{
    public ModelLevel2 Sub1 { get; set; } = null!;
}

public class WrappedModel
{
    public Lazy<ModelLevel2> LazySub { get; set; } = null!;
}

public class SelfReferencingModel
{
    public SelfReferencingModel Self { get; set; } = null!;
}