using TSpec.Assert;

namespace TSpec.Test.Given;

public class WhenGivenConstrainedInt : Spec<int>
{
    [Fact]
    //i : [1000..1100]
    public void ThenConstraintIsAppliedToAllInts()
        => Given<string>(s => s.ToUpper())
        .Given<TimeSpan>(ts => ts < TimeSpan.FromHours(1) ? TimeSpan.FromHours(2) - ts : ts)
        .And<int>(i => i < 1000 || i > 1100 ? 1000 + i % 100 : i)
        .When(_ => _)
        .Then().Result.Is().GreaterThan(999).and.LessThan(1100)
        .And(A<TimeSpan>()).Is().not.LessThan(TimeSpan.FromHours(1))
        .And(A<string>()).Is().UpperCase().and.not.LowerCase();
}
