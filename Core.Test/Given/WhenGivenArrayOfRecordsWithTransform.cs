using TSpec.Assert;
using TSpec.Test.TestData;

namespace TSpec.Test.Given;

public class WhenGivenArrayOfRecordsWithTransform : Spec<MyService, MyModel[]>
{
    private const string _myString = "MyString";

    public WhenGivenArrayOfRecordsWithTransform()
        => When(_ => _.GetModels())
        .Given().Three<MyModel>((_, i) => _ with { Name = _myString });

    [Fact]
    public void ThenGetTheArrayOfModels()
        => Result.Is(Three<MyModel>());

    [Fact]
    public void ThenTransformIsAppliedToReferencedValues()
    {
        Then();
        A<MyModel>().Name.Is(_myString);
    }
}

public class WhenGivenOneSpecificModel : Spec<MyService, MyModel[]>
{
    public WhenGivenOneSpecificModel()
        => When(_ => _.GetModels())
        .Given().One(new MyModel() { Name = "abc" });

    [Fact]
    public void ThenCanReferenceThatModel()
        => Result.Has().OneItem().that.Name.Is("abc");
}

public class WhenGivenSomeSpecificModels : Spec<MyService, MyModel[]>
{
    public WhenGivenSomeSpecificModels()
        => When(_ => _.GetModels()).Given().Some([new MyModel() { Name = "abc" }]);

    [Fact]
    public void ThenCanReferenceThoseModels()
        => Result.Has().OneItem().that.Name.Is("abc");
}

public class WhenGivenFiveModels : Spec<MyService, MyModel[]>
{
    public WhenGivenFiveModels()
        => When(_ => _.GetModels()).Given().Five<MyModel>();

    [Fact]
    public void ThenCanReferenceThoseModels()
        => Result.Has().FiveItems().that.third.Is(AThird<MyModel>());
}