using TSpec.Assert;
using TSpec.Test.TestData;

namespace TSpec.Test.Given;

public class WhenGivenConstraint : Spec<MyService, MyModel> 
{
    [Fact]
    public void GivenDefaultIntBetween_90_and_100()
        => Given<int>(_ => _ % 10 + 90).When(_ => _.GetModel()).Then().Result.Id.Is().GreaterThan(89).and.LessThan(100);

    [Fact]
    public void GivenAnIntBetween_90_and_100()
        => Given().An<int>(_ => _ % 10 + 90)
        .And<MyModel>(_ => _.Id = An<int>())
        .When(_ => _.GetModel())
        .Then().Result.Id.Is().GreaterThan(89).and.LessThan(100);
}