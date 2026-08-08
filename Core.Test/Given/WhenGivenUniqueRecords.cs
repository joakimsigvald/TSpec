using TSpec.Assert;
using TSpec.Test.TestData;
using Xunit.Sdk;

namespace TSpec.Test.Given;

public class WhenGivenUniqueRecords : Spec<MyRecord[]>
{
    [Fact]
    public void WithEnoughValueSpace_ThenGenerateUniqueModelArray()
    {
        int range = 10;
        When(_ => Five<MyRecord>()).Using<int>(i => i % range).Using("Abc", For.Input)
            .Then().Result.Is().Distinct()
            .and.Has().All(m => m.Id >= 0 && m.Id < range);
        Specification.Is(
            """
            Using int is i % range
              and "Abc" for Input
            When five MyRecords
            Then Result is distinct
                and has all m.Id >= 0 && m.Id < range
            """);
    }

    [Fact]
    public void WithNotEnoughValueSpace_ThenThrowXUnitException()
    {
        int range = 4;
        Xunit.Assert.Throws<XunitException>(() => 
        When(_ => Five<MyRecord>()).Using<int>(i => i % range).Using("Abc", For.Input)
            .Then().Result.Is().Distinct()
            .and.Has().All(m => m.Id >= 0 && m.Id < range));
    }
}