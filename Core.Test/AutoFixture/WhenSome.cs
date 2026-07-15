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
                Using some MyModel
                When _.List()
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
@"Using some MyModel
  and one MyModel
When _.List()
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
@"Using some MyModel
  and three MyModel
When _.List()
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
@"Using some MyModel
  and Array.Empty<MyModel>
When _.List()
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
@"Using some MyModel
  and many MyModel
When _.List()
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
@"Using some MyModel
  and one MyModel
  and some MyModel
When _.List()
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
@"Using some MyModel
  and two MyModel
  and some MyModel
When _.List()
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
@"Using some MyModel
  and Array.Empty<MyModel>
  and some MyModel
When _.List()
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
@"Using some MyModel
  and many MyModel
  and some MyModel
When _.List()
Then Result has count 2");
        }
    }
}