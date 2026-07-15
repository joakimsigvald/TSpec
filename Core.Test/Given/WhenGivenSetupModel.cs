using TSpec.Assert;
using TSpec.Test.TestData;

namespace TSpec.Test.Given;

public class WhenGivenSetupModel : Spec<MyService, MyModel>
{
    private static readonly MyModel _myModel = new() { Name = "My model" };

    [Fact]
    public void ThenCanApplySpecificValueToSecondPosition()
    {
        Given<IMyRepository>().That(_ => _.GetModel()).Returns(ASecond<MyModel>)
            .When(_ => _.GetModel())
            .Given().ASecond(_myModel)
            .Then().Result.Is(_myModel);
        Specification.Is(
            """
            Given a second MyModel is _myModel
              and IMyRepository.GetModel() returns a second MyModel
            When _.GetModel()
            Then Result is _myModel
            """);
    }

    [Fact]
    public void ThenCanApplyNullAsSpecificValueToSecondPosition()
    {
        Given<IMyRepository>().That(_ => _.GetModel()).Returns(() => ASecond<MyModel>())
            .When(_ => _.GetModel())
            .Given().ASecond((MyModel?)null)
            .Then().Result.Is().Null();
        Specification.Is(
            """
            Given a second MyModel is (MyModel?)null
              and IMyRepository.GetModel() returns a second MyModel
            When _.GetModel()
            Then Result is null
            """);
    }

    [Fact]
    public void ThenOnlyApplyNullAsSpecificValueToMentionedPosition()
    {
        Given<IMyRepository>().That(_ => _.GetModels()).Returns(() => One<MyModel>())
            .When(_ => _.GetModel()) //Unspecified default value
            .Given().A((MyModel?)null) //First specific model is null
            .Then().Result.Is().not.Null(); //Not the provide null mentioned value
        Specification.Is(
            """
            Given a MyModel is (MyModel?)null
              and IMyRepository.GetModels() returns one MyModel
            When _.GetModel()
            Then Result is not null
            """);
    }
}