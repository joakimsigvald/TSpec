using TSpec.Assert;
using TSpec.Test.TestData;

namespace TSpec.Test.Given;

public class WhenGivenSetupValueWithDefault : Spec<MyService, int>
{
    private const int _defaultId = 1;

    [Fact]
    public void GivenDefaultNotOverridden()
    {
        Using(_defaultId)
            .Given<IMyRepository>().That(_ => _.GetNextId()).Returns(() => ASecond<int>())
            .When(_ => _.GetNextId())
            .Then().Result.Is(_defaultId);
        Specification.Is(
            """
            Using _defaultId
            Given IMyRepository.GetNextId() returns a second int
            When _.GetNextId()
            Then Result is _defaultId
            """);
    }

    [Fact]
    public void GivenDefaultIsOverridden()
    {
        Given<IMyRepository>().That(_ => _.GetNextId()).Returns(() => ASecond<int>())
            .When(_ => _.GetNextId())
            .Using(_defaultId)
            .Given().ASecond(2)
            .Then().Result.Is(2);
        Specification.Is(
            """
            Using _defaultId
            Given a second int is 2
              and IMyRepository.GetNextId() returns a second int
            When _.GetNextId()
            Then Result is 2
            """);
    }
}