using TSpec.Assert;
using TSpec.Test.TestData;

namespace TSpec.Test.Given;

public abstract class WhenGetArrayOfModels : Spec<MyService, MyModel[]>
{
    protected WhenGetArrayOfModels() => When(_ => _.GetModels()).Given<IMyRepository>().That(_ => _.GetModels()).Returns(A<MyModel[]>);

    public class GivenMentionOfAnyArray_FollowedBy_MentionOfArrayOfSpecificLength : WhenGetArrayOfModels
    {
        public GivenMentionOfAnyArray_FollowedBy_MentionOfArrayOfSpecificLength()
            => Using(Two<MyModel>).Given<MyModel>(_ => _.Name = A<string>());

        [Fact]
        public void ThenGetArrayOfSpecificLength()
        {
            Then().Result.Is(Two<MyModel>()).And(Result).First().Name.Is(The<string>());
            Specification.Is(
                """
            Given MyModel has Name = a string
            Using two MyModel
              and IMyRepository.GetModels() returns a MyModel[]
            When _.GetModels()
            Then Result is two MyModel
              and Result.First().Name is the string
            """);
        }
    }

    public class GivenZero : WhenGetArrayOfModels
    {
        public GivenZero() => Given().Zero<MyModel>();
        [Fact] public void ThenReturnedArrayHasZeroElements() => Then().Result.Length.Is(0);
    }

    public class GivenOne : WhenGetArrayOfModels
    {
        public GivenOne() => Given().One<MyModel>();
        [Fact] public void ThenReturnedArrayHasOneElement() => Then().Result.Length.Is(1);
    }

    public class GivenTwo : WhenGetArrayOfModels
    {
        public GivenTwo() => Given().Two<MyModel>();
        [Fact] public void ThenReturnedArrayHasTwoElements() => Then().Result.Length.Is(2);
    }

    public class GivenThree : WhenGetArrayOfModels
    {
        public GivenThree() => Given().Three<MyModel>();
        [Fact] public void ThenReturnedArrayHasThreeElements() => Then().Result.Length.Is(3);
    }

    public class GivenFour : WhenGetArrayOfModels
    {
        public GivenFour() => Given().Four<MyModel>();
        [Fact] public void ThenReturnedArrayHasFourElements() => Then().Result.Length.Is(4);
    }

    public class GivenFive : WhenGetArrayOfModels
    {
        public GivenFive() => Given().Five<MyModel>();
        [Fact] public void ThenReturnedArrayHasFiveElements() => Then().Result.Length.Is(5);
    }

    public class GivenOneWithSetup : WhenGetArrayOfModels
    {
        public GivenOneWithSetup() => Given().One<MyModel>(_ => _.Id = An<int>());

        [Fact] public void ThenApplySetupToReturnedArray() => Then().Result.Single().Id.Is(The<int>());
    }

    public class GivenTwoWithSetup : WhenGetArrayOfModels
    {
        public GivenTwoWithSetup() => Given().Two<MyModel>(_ => _.Id = An<int>());

        [Fact]
        public void GivenTwoWithSetup_ThenApplySetupToReturnedArray()
            => Then().Result.Has().All(m => m.Id == The<int>());
    }

    public class GivenThreeWithIndexedSetup : WhenGetArrayOfModels
    {
        public GivenThreeWithIndexedSetup() => Given().Three<MyModel>((_, i) => _.Id = i + 1);

        [Fact] public void ThenApplySetupToReturnedArray() => Then().Result.Has().All((m, i) => m.Id == i + 1);
    }

    public class GivenFourWithTransform : WhenGetArrayOfModels
    {
        public GivenFourWithTransform() => Given().Four<MyModel>(_ => _ with { Id = An<int>() });

        [Fact] public void ThenApplyTransformToReturnedArray() => Then().Result.Has().All(m => m.Id == The<int>());
    }

    public class GivenFiveWithIndexedTransform : WhenGetArrayOfModels
    {
        public GivenFiveWithIndexedTransform() => Given().Five<MyModel>((_, i) => _ with { Id = i + 1 });

        [Fact] public void ThenApplyTransformToReturnedArray() => Then().Result.Has().All((m, i) => m.Id == i + 1);
    }

    public class GivenTwoItemsWithIndexedSetup : WhenGetArrayOfModels
    {
        public GivenTwoItemsWithIndexedSetup() => Given().Two<MyModel>((_, i) => _.Id = i + 1);

        [Fact] public void ThenFirstModelHasSetup() => Result[0].Id.Is(TheFirst<MyModel>().Id);
        [Fact] public void ThenSecondModelHasSetup() => Result[1].Id.Is(TheSecond<MyModel>().Id);
    }

    public class GivenTwoItemsWithTransform : WhenGetArrayOfModels
    {
        public GivenTwoItemsWithTransform() => Given().Two<MyModel>(_ => _ with { Id = 123 });

        [Fact] public void ThenFirstModelHasSetup() => Result[0].Id.Is(TheFirst<MyModel>().Id);
        [Fact] public void ThenSecondModelHasSetup() => Result[1].Id.Is(TheSecond<MyModel>().Id);
    }

    public class GivenTwoItemsWithIndexedTransform : WhenGetArrayOfModels
    {
        public GivenTwoItemsWithIndexedTransform() => Given().Two<MyModel>((_, i) => _ with { Id = i + 1 });

        [Fact] public void ThenFirstModelHasSetup() => Result[0].Id.Is(TheFirst<MyModel>().Id);
        [Fact] public void ThenSecondModelHasSetup() => Result[1].Id.Is(TheSecond<MyModel>().Id);
    }
}