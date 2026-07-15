using TSpec.Assert;

namespace TSpec.Test.AutoFixture.Generator;

public abstract class MyAbstractClass
{
    public abstract string GetValue();
}

public interface MyInterface
{
    string GetValue();
}

public class WhenGenerateAbstractClass : Spec<MyAbstractClass>
{
    [Fact]
    public void GivenSUT_ThenCanBeCreated()
        => When(_ => _).Then().Result.GetValue().Is().not.NullOrEmpty();

    [Fact]
    public void GivenSingle_ThenIsNotNull()
        => A<MyAbstractClass>().GetValue().Is().not.NullOrEmpty();
}
public class WhenGenerateInterface : Spec<MyInterface>
{
    [Fact]
    public void GivenSUT_ThenCanBeCreated()
        => When(_ => _).Then().Result.GetValue().Is().not.NullOrEmpty();

    [Fact]
    public void GivenSingle_ThenIsNotNull()
        => A<MyInterface>().GetValue().Is().not.NullOrEmpty();
}