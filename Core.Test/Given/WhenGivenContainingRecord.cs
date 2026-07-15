using TSpec.Assert;
using TSpec.Test.TestData;
using static TSpec.Test.Given.WhenGivenContainingRecord;

namespace TSpec.Test.Given;

public class WhenGivenContainingRecord : Spec<MyService, ContainingRecord>
{
    [Fact]
    public void GivenDefaultRecordSetupContainedInAnotherRecord_ThenUseDefault()
    {
        Using(new MyRecord(1, A<string>()))
            .When(_ => MyService.Echo(A<ContainingRecord>()))
            .Then().Result.MyRecord.Name.Is(The<string>());
    }

    [Fact]
    public void GivenDefaultRecordInstanceContainedInAnotherRecord_ThenUseDefault()
    {
        Given<MyRecord>(_ => _ with { Name = A<string>() })
            .When(_ => MyService.Echo(A<ContainingRecord>()))
            .Then().Result.MyRecord.Name.Is(The<string>());
    }

    public record ContainingRecord(MyRecord MyRecord);
}