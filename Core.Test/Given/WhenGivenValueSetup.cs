using TSpec.Assert;
using TSpec.Test.TestData;

namespace TSpec.Test.Given;

public class WhenGivenValueSetup : Spec<MyService, MyModel>
{
    [Fact]
    public void AsFirstSentence_ThenUseSetupInPipeline()
    {
        Given<MyModel>(_ => _.Name = A<string>())
            .When(_ => MyService.Echo(A<MyModel>()))
            .Then().Result.Name.Is(The<string>());
        Specification.Is(
            """
            Given MyModel has Name = a string
            When MyService.Echo(a MyModel)
            Then Result.Name is the string
            """);
    }

    [Fact]
    public void AsSecondSentence_ThenUseSetupInPipeline()
    {
        Given<IMyRepository>().That(_ => _.GetModel()).Returns(A<MyModel>)
            .And<MyModel>(_ => _.Name = A<string>())
            .When(_ => _.GetModel())
            .Then().Result.Name.Is(The<string>());
        Specification.Is(
            """
            Given MyModel has Name = a string
              and IMyRepository.GetModel() returns a MyModel
            When _.GetModel()
            Then Result.Name is the string
            """);
    }

    [Fact]
    public void WithArithmeticExpression_ThenParseValues()
    {
        Given<MyModel>(_ => _.Name = A<string>() + ASecond<string>())
            .When(_ => MyService.Echo(A<MyModel>()))
            .Then().Result.Name.Does().StartWith(The<string>());
        Specification.Is(
            """
            Given MyModel has Name = a string + a second string
            When MyService.Echo(a MyModel)
            Then Result.Name starts with the string
            """);
    }

    [Fact]
    public void WithLineBreaks_ThenMergeToOneLine()
    {
        Using("abc"
            .ToUpper()
            .ToLower())
            .When(_ => _.GetModel())
            .Then().Result.Name.Has(_ => char.IsLower(_[0]));
        Specification.Is(
            """
            Using "abc".ToUpper().ToLower()
            When _.GetModel()
            Then Result.Name has char.IsLower(_[0])
            """);
    }

    [Fact]
    public void WithLongLine_ThenInsertLineBreaks()
    {
        Using("abc".ToUpper().ToLower().ToUpper().ToLower()
            .ToUpper().ToLower().ToUpper().ToLower().ToUpper()
            .ToLower().ToUpper().ToLower().ToUpper().ToLower())
            .When(_ => _.GetModel())
            .Then().Result.Name.Has(_ => char.IsLower(_[0]));
        Specification.Is(
            """
            Using "abc".ToUpper().ToLower().ToUpper().ToLower().ToUpper().ToLower().ToUpper(
                  ).ToLower().ToUpper().ToLower().ToUpper().ToLower().ToUpper().ToLower()
            When _.GetModel()
            Then Result.Name has char.IsLower(_[0])
            """);
    }
}

public abstract class WhenGivenValueTransform : Spec<MyModel>
{
    public class GivenTransformFails : WhenGivenValueTransform
    {
        [Fact]
        public void ThenThrowsSetupFailed() 
        {
            var ex = Xunit.Assert.Throws<SetupFailed>(() => 
            Given().A<MyModel>(_ => throw new ApplicationException("Error"))
            .When(_ => A<MyModel>()).Then());
        }
    }
}