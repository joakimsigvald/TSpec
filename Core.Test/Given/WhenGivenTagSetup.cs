using TSpec.Assert;
using TSpec.Test.TestData;

namespace TSpec.Test.Given;

public class WhenGivenTagSetup : Spec<MyService, MyModel> 
{
    [Fact]
    public void ApplyAction() 
    { 
        Tag<MyModel> model = new();
        Using(model, For.Subject)
            .Given(model).Has(_ => _.Name = A<string>())
            .And(model).Has(_ => _.Id = An<int>())
            .When(_ => _.GetModel())
            .Then().Result.Name.Is(The<string>()).And(Result).Id.Is(The<int>());
        Specification.Is(
            """
            Using model for Subject
            Given model has Id = an int
              and model has Name = a string
            When _.GetModel()
            Then Result.Name is the string
              and Result.Id is the int
            """);
    }

    [Fact]
    public void ApplyTransform()
    {
        Tag<MyModel> model = new();
        Given(model).Has(_ => _ with { Name = A<string>(), Id = An<int>() })
            .And<IMyRepository>().Returns(model)
            .When(_ => _.GetModel())
            .Then().Result.Name.Is(The<string>()).And(Result).Id.Is(The<int>());
        Specification.Is(
            """
            Given model has Name = a string, Id = an int
              and IMyRepository returns model
            When _.GetModel()
            Then Result.Name is the string
              and Result.Id is the int
            """);
    }
}