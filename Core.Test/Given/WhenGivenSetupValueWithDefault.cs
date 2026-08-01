using TSpec.Assert;
using TSpec.Test.TestData;

namespace TSpec.Test.Given;

public class WhenGivenSetupValueWithDefault : Spec<MyService, int>
{
    private const int DefaultId = 1;

    [Fact]
    public void GivenDefaultNotOverridden()
    {
        Using(DefaultId)
            .Given<IMyRepository>().That(_ => _.GetNextId()).Returns(() => ASecond<int>())
            .When(_ => _.GetNextId())
            .Then().Result.Is(DefaultId);
        Specification.Is(
            """
            Using DefaultId
            Given IMyRepository.GetNextId() returns a second int
            When GetNextId()
            Then Result is DefaultId
            """);
    }

    [Fact]
    public void GivenDefaultIsOverridden()
    {
        Given<IMyRepository>().That(_ => _.GetNextId()).Returns(() => ASecond<int>())
            .When(_ => _.GetNextId())
            .Using(DefaultId)
            .Given().ASecond(2)
            .Then().Result.Is(2);
        Specification.Is(
            """
            Using DefaultId
            Given a second int is 2
              and IMyRepository.GetNextId() returns a second int
            When GetNextId()
            Then Result is 2
            """);
    }
}