using TSpec.Assert;

namespace TSpec.Test.AutoFixture;

public class WhenGetWithSetup : Spec<MyMappingRetreiver, MyModel>
{
    public WhenGetWithSetup()
        => When(_ => _.Get(An<int>()))
        .Given<IMyRepository>().That(_ => _.Get(The<int>())).Returns(() => An<MyModel>(_ => _.Name = A<string>()));

    [Fact]
    public void Setup_CanBeProvided_ToPreviouslyMentionedModel()
    {
        Given<IMyMapper>().That(_ => _.Map(A<MyModel>())).Returns(() => The<MyModel>())
            .Then().Result.Name.Is(The<string>());
        Specification.Is(
@"Given IMyRepository.Get(the int) returns an MyModel { Name = a string }
  and IMyMapper.Map(a MyModel) returns the MyModel
When _.Get(an int)
Then Result.Name is the string");
    }

    [Fact]
    public void Setup_CanBeProvided_MoreThanOnce_ToSameModel()
    {
        Given<IMyMapper>().That(_ => _.Map(The<MyModel>()))
            .Returns(() => AFirst<MyModel>(_ => _.Id = An<int>()))
            .Then().Result.Name.Is(The<string>()).And(Result).Id.Is(The<int>());
        Specification.Is(
            """
            Given IMyRepository.Get(the int) returns an MyModel { Name = a string }
              and IMyMapper.Map(the MyModel) returns a first MyModel { Id = an int }
            When _.Get(an int)
            Then Result.Name is the string
              and Result.Id is the int
            """);
    }

    [Fact]
    public void Setup_CannotBeProvided_AfterThen()
        => Xunit.Assert.Throws<SetupFailed>(
            () => Given<IMyMapper>().That(_ => _.Map(A<MyModel>())).Returns(() => The<MyModel>())
            .Then().Result.Is(A<MyModel>(_ => _.Name = "abc")));
}