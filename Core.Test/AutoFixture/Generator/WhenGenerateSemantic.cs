using TSpec.Assert;
using TSpec.Semantic;
using TSpec.Test.AutoFixture.Primitives;

namespace TSpec.Test.AutoFixture.Generator;

public abstract class WhenGenerateSemantic<TSemantic, TPrimitive>(
    int maxDistinct, Action<TSemantic> verify) : Spec<TSemantic>
    where TSemantic : Semantic<TPrimitive>, ISemantic<TPrimitive>
{
    [Fact]
    public void GivenSUT_ThenIsNotDefault()
        => When(_ => _).Then().Result.Value.Is().Not(default(TSemantic));

    [Fact]
    public void GivenMaxDistinctValues_ThenAreDifferent()
        => Enumerable.Range(1, maxDistinct).Select(n => Any<TSemantic>()).Is().Distinct().and.Has().All(verify);

    [Fact]
    public void ThenImplicitlyCastsToPrimitive()
        => A<TSemantic>().Value.Is((TPrimitive)The<TSemantic>());
}

public partial class WhenGenerateEmail() : WhenGenerateSemantic<Email, string>(
    100, e => ValidEmail.Verify(e).Is(true));

public partial class WhenGeneratePhoneNumber() : WhenGenerateSemantic<PhoneNumber, string>(100, p => ValidPhoneNumber.Verify(p).Is(true));

public partial class WhenGenerateAge() : WhenGenerateSemantic<Age, int>(
    100, a => a.Value.Is().not.LessThan(0).and.LessThan(120));