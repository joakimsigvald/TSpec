using TSpec.Assert;

namespace TSpec.Test.Assert;

public class WhenDouble : Spec<double>
{
    [Fact] public void IsAroundSame() => When(_ => A(_)).Then().Result.Is().Around(The<double>(), 1);
    [Fact] public void IsAroundUpperBound() => When(_ => 1).Then().Result.Is().Around(2, 1);
    [Fact] public void IsAroundLowerBound() => When(_ => 1).Then().Result.Is().Around(0, 1);
    [Fact] public void IsNotAroundAboveUpperBound() => When(_ => 1).Then().Result.Is().not.Around(2.001, 1);
    [Fact] public void IsNotAroundBelowLowerBound() => When(_ => 1).Then().Result.Is().not.Around(-0.001, 1);
    [Fact] public void IsNotAroundUpperBoundFail() 
        => Xunit.Assert.Throws<Xunit.Sdk.XunitException>(() => When(_ => 1).Then().Result.Is().not.Around(2, 1));
    [Fact] public void IsNotAroundLowerBoundFail() 
        => Xunit.Assert.Throws<Xunit.Sdk.XunitException>(() => When(_ => 1).Then().Result.Is().not.Around(0, 1));
    [Fact] public void IsAroundAboveUpperBoundFail() 
        => Xunit.Assert.Throws<Xunit.Sdk.XunitException>(() => When(_ => 1).Then().Result.Is().Around(2.001, 1));
    [Fact] public void IsAroundBelowLowerBoundFail() 
        => Xunit.Assert.Throws<Xunit.Sdk.XunitException>(() => When(_ => 1).Then().Result.Is().Around(-0.001, 1));
}