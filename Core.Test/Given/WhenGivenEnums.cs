using TSpec.Assert;
using TSpec.Test.TestData;

namespace TSpec.Test.Given;

public enum SparseEnum { One = 2, Five = 5, Ten = 10 }

public enum SingleEnum { Only }

public class WhenGivenEnums : Spec<MyEnum[]>
{
    [Fact]
    public void GivenTwoMentions_ThenValuesDiffer()
        => A<MyEnum>().Is().Not(ASecond<MyEnum>());

    [Fact]
    public void GivenThreeEnums_ThenValuesAreDistinct()
        => Three<MyEnum>().Is().Distinct();

    [Fact]
    public void GivenSparseEnum_ThenValuesAreDefinedMembers()
        => Three<SparseEnum>().Is().Distinct()
            .and.Has().All(e => Enum.IsDefined(e));

    [Fact]
    public void GivenFewerMembersThanMentions_ThenValuesRepeat()
        => Three<SingleEnum>().Has().All(e => e == SingleEnum.Only);
}
