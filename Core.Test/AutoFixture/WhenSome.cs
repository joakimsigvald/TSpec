using TSpec.Assert;

namespace TSpec.Test.AutoFixture;

public class WhenSome : Spec<MyRetriever, MyModel[]>
{
    public WhenSome() => Using(Some<MyModel>).When(_ => _.List());

    public class GivenNoOtherReference : WhenSome
    {
        [Fact]
        public void ThenArrayHasTwoElements()
        {
            Result.Has().Count(2);
            Specification.Is(
                """
                Using some MyModels
                When List()
                Then Result has count 2
                """);
        }
    }

    public class GivenOneIsMentionedAfter : WhenSome
    {
        public GivenOneIsMentionedAfter() => Using(One<MyModel>);

        [Fact]
        public void ThenCountIsOne()
        {
            Result.Has().Count(1);
            Specification.Is(
@"Using some MyModels
  and one MyModel
When List()
Then Result has count 1");
        }
    }

    public class GivenThreeIsMentionedAfter : WhenSome
    {
        public GivenThreeIsMentionedAfter() => Using(Three<MyModel>);

        [Fact]
        public void ThenCountIsThree()
        {
            Result.Has().Count(3);
            Specification.Is(
@"Using some MyModels
  and three MyModels
When List()
Then Result has count 3");
        }
    }

    public class GivenEmptyIsMentionedAfter : WhenSome
    {
        public GivenEmptyIsMentionedAfter() => Using(Array.Empty<MyModel>);

        [Fact]
        public void ThenCountIsZero()
        {
            Result.Has().Count(0);
            Specification.Is(
@"Using some MyModels
  and Array.Empty<MyModel>
When List()
Then Result has count 0");
        }
    }

    public class GivenManyIsMentionedAfter : WhenSome
    {
        public GivenManyIsMentionedAfter() => Using(Many<MyModel>);

        [Fact]
        public void ThenCountIsTwo()
        {
            Result.Has().Count(2);
            Specification.Is(
@"Using some MyModels
  and many MyModels
When List()
Then Result has count 2");
        }
    }

    public class GivenOneIsMentionedBefore : WhenSome
    {
        public GivenOneIsMentionedBefore() => Using(One<MyModel>).And(Some<MyModel>);

        [Fact]
        public void ThenCountIsOne()
        {
            Result.Has().Count(1);
            Specification.Is(
@"Using some MyModels
  and one MyModel
  and some MyModels
When List()
Then Result has count 1");
        }
    }

    public class GivenTwoIsMentionedBefore : WhenSome
    {
        public GivenTwoIsMentionedBefore() => Using(Two<MyModel>).And(Some<MyModel>);

        [Fact]
        public void ThenCountIsTwo()
        {
            Result.Has().Count(2);
            Specification.Is(
@"Using some MyModels
  and two MyModels
  and some MyModels
When List()
Then Result has count 2");
        }
    }

    public class GivenEmptyIsMentionedBefore : WhenSome
    {
        public GivenEmptyIsMentionedBefore() => Using(Array.Empty<MyModel>).And(Some<MyModel>);

        [Fact]
        public void ThenCountIsTwo()
        {
            Result.Has().Count(2);
            Specification.Is(
@"Using some MyModels
  and Array.Empty<MyModel>
  and some MyModels
When List()
Then Result has count 2");
        }
    }

    public class GivenManyIsMentionedBefore : WhenSome
    {
        public GivenManyIsMentionedBefore() => Using(Many<MyModel>).And(Some<MyModel>);

        [Fact]
        public void ThenCountIsTwo()
        {
            Result.Has().Count(2);
            Specification.Is(
@"Using some MyModels
  and many MyModels
  and some MyModels
When List()
Then Result has count 2");
        }
    }
}