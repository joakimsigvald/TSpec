using TSpec.Assert;

namespace TSpec.Test.AutoFixture;

public class WhenAnyNumberOf : Spec<MyRetriever, MyModel[]>
{
    public WhenAnyNumberOf() => Using(AnyNumberOf<MyModel>).When(_ => _.List());

    public class GivenNoOtherReference : WhenAnyNumberOf
    {
        [Fact]
        public void ThenArrayHasOneElements()
        {
            Result.Has().Count(1);
            Specification.Is(
                """
                Using any number of MyModel
                When _.List()
                Then Result has count 1
                """);
        }
    }

    public class GivenZeroIsMentionedBefore : WhenAnyNumberOf
    {
        public GivenZeroIsMentionedBefore() => Using(Zero<MyModel>).And(AnyNumberOf<MyModel>);

        [Fact]
        public void ThenCountIsZero()
        {
            Result.Has().Count(0);
            Specification.Is(
                """
                Using any number of MyModel
                  and zero MyModel
                  and any number of MyModel
                When _.List()
                Then Result has count 0
                """);
        }
    }
}
