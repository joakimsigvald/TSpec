using TSpec.Assert;

namespace TSpec.Test.AutoFixture;

public abstract class WhenSomeOther : Spec<MyRetriever, MyModel[]>
{
    public WhenSomeOther() => Using(() => SomeOther<MyModel>()).When(_ => _.List());

    public class GivenNoOtherReference : WhenSomeOther
    {
        [Fact]
        public void ThenArrayHasTwoElements()
        {
            Result.Has().Count(2);
            Specification.Is(
                """
                Using some other MyModel
                When _.List()
                Then Result has count 2
                """);
        }
    }

    public class GivenOneIsMentionedAfter : WhenSomeOther
    {
        public GivenOneIsMentionedAfter() => Using(One<MyModel>);

        [Fact]
        public void ThenCountIsThatOne()
        {
            Result.Has().OneItem().that.Is(The<MyModel>());
            Specification.Is(
@"Using some other MyModel
  and one MyModel
When _.List()
Then Result has one item that is the MyModel");
        }
    }

    public class GivenThreeIsMentionedAfter : WhenSomeOther
    {
        public GivenThreeIsMentionedAfter() => Using(Three<MyModel>);

        [Fact]
        public void ThenCountIsThree()
        {
            Result.Has().Count(3);
            Specification.Is(
@"Using some other MyModel
  and three MyModel
When _.List()
Then Result has count 3");
        }
    }

    public class GivenOneIsMentionedBefore : WhenSomeOther
    {
        public GivenOneIsMentionedBefore() => Using(One<MyModel>).And(Some<MyModel>);

        [Fact]
        public void ThenCountIsOne()
        {
            Result.Has().Count(1);
            Specification.Is(
@"Using some other MyModel
  and one MyModel
  and some MyModel
When _.List()
Then Result has count 1");
        }
    }

    public class GivenTwoIsMentionedBefore : WhenSomeOther
    {
        public GivenTwoIsMentionedBefore() => Using(Two<MyModel>).And(Some<MyModel>);

        [Fact]
        public void ThenTwoModelsAreThoseTwoModels()
        {
            Result.Is(Two<MyModel>());
            Specification.Is(
@"Using some other MyModel
  and two MyModel
  and some MyModel
When _.List()
Then Result is two MyModel");
        }
    }

    public class GivenSpecificNumberOfOther : WhenSomeOther
    {
        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(3)]
        [InlineData(10)]
        public void ThenGetThatNumberOfOtherElements(int count) 
        {
            Then();
            SomeOther<MyModel>(count).Length.Is(count);
            SomeOther<MyModel>().Is().not.Like(SomeOther<MyModel>());
        }
    }
}