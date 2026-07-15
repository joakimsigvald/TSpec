using TSpec.Assert;
using TSpec.Test.AutoFixture;

namespace TSpec.Test.Given;

public class WhenGivenConstrainedClass : Spec<MyModel>
{
    [Fact]
    //i : [1000..1100]
    public void ThenConstraintIsAppliedToAllInstancess()
        => Given<MyModel>(m => m.Id = m.Id < 1000 || m.Id > 1100 ? 1000 + m.Id % 100 : m.Id)
        .When(_ => _)
        .Then().Result.Id.Is().GreaterThan(999).and.LessThan(1100);
}
