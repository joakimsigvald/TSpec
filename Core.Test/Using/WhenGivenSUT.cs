using TSpec.Assert;
using TSpec.Test.TestData;

namespace TSpec.Test.Using;

public class WhenGivenSUT : Spec<MyService, (int, string)>
{
    public WhenGivenSUT()
        => Using(new MyService(An<IMyRepository>(), new MySettings { ConnectionString = A<string>() }, () => DateTime.Now))
            .Given<IMyRepository>().That(_ => _.GetNextId()).Returns(An<int>)
        .When(_ => new(_.GetNextId(), _.GetConnectionString()));

    [Fact]
    public void ThenUseSUT()
    {
        Then().Result.Is((The<int>(), The<string>()));
        Specification.Is(
            """
            Using new MyService(an IMyRepository, new MySettings { ConnectionString = a
                  string }, DateTime.Now)
            Given IMyRepository.GetNextId() returns an int
            When new(_.GetNextId(), _.GetConnectionString())
            Then Result is (the int, the string)
            """);
    }
}