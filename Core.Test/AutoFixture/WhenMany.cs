using TSpec.Assert;

namespace TSpec.Test.AutoFixture;

public class WhenMany : Spec<MyRetriever, MyModel[]>
{
    public WhenMany() => When(_ => _.List());

    public class GivenReferringManyTwice : WhenMany
    {
        public GivenReferringManyTwice() => Given(Many<MyModel>());

        [Fact]
        public void ThenCanRetrieveThatArray()
        {
            Result.Is(Many<MyModel>());
            Specification.Is(
@"Given many MyModels
When List()
Then Result is many MyModels");
        }

        [Fact]
        public void ThenArrayHasThreeElements()
        {
            Result.Has().Count(3);
            Specification.Is(
@"Given many MyModels
When List()
Then Result has count 3");
        }

        [Fact]
        public void ThenDifferentReferencesToMany_AreTheSameArray()
        {
            Then(Many<MyModel>()).Is(Many<MyModel>());
            Specification.Is(
@"Given many MyModels
When List()
Then many MyModels is many MyModels");
        }

        [Fact]
        public void ThenWithSubjectAsString_UseThatSubject()
        {
            Then("Hej").Is("Hej");
            Specification.Is(
"""
Given many MyModels
When List()
Then "Hej" is "Hej"
""");
        }
    }

    public class GivenReferringManyOfHigherCountSecondTime : WhenMany
    {
        public GivenReferringManyOfHigherCountSecondTime() => Given(Two<MyModel>());

        [Fact]
        public void ThenItIsDifferentFromFirst()
        {
            Result.Is().Not(Three<MyModel>());
            Specification.Is(
@"Given two MyModels
When List()
Then Result is not three MyModels");
        }

        [Fact]
        public void ThenArrayHasOriginalCount()
        {
            Result.Has().Count(2);
            Specification.Is(
@"Given two MyModels
When List()
Then Result has count 2");
        }

        [Fact]
        public void ThenLastElementIsCreated()
        {
            Then(TheThird<MyModel>()).Is(Three<MyModel>().Last());
            Specification.Is(
@"Given two MyModels
When List()
Then the third MyModel is three MyModels' Last()");
        }

        [Fact]
        public void ThenDifferentReferencesToManyOfSameCount_HaveSameElements()
        {
            Then(Three<MyModel>()).Is().EqualTo(Three<MyModel>());
            Specification.Is(
@"Given two MyModels
When List()
Then three MyModels is equal to three MyModels");
        }
    }

    public class GivenReferringManyOfLowerCountSecondTime : WhenMany
    {
        public GivenReferringManyOfLowerCountSecondTime() => Given(Four<MyModel>());

        [Fact]
        public void ThenItIsDifferentFromFirst()
        {
            Result.Is().Not(Three<MyModel>());
            Specification.Is(
@"Given four MyModels
When List()
Then Result is not three MyModels");
        }

        [Fact]
        public void ThenArrayHasOriginalCount()
        {
            Result.Has().Count(4);
            Specification.Is(
@"Given four MyModels
When List()
Then Result has count 4");
        }

        [Fact]
        public void ThenDifferentReferencesToManyOfSameCount_HaveSameElements()
        {
            Then(Three<MyModel>()).Is().EqualTo(Three<MyModel>());
            Specification.Is(
@"Given four MyModels
When List()
Then three MyModels is equal to three MyModels");
        }
    }

    public class GivenMentionManyAfterTwo : WhenMany
    {
        [Fact]
        public void ThenReturnTwoAsMany()
        {
            Given<IMyRepository>().That(_ => _.List()).Returns(Many<MyModel>)
                .Using(Two<MyModel>).Then().Result.Has().Count(2);
            Specification.Is(
                """
                Using two MyModels
                Given IMyRepository.List() returns many MyModels
                When List()
                Then Result has count 2
                """);
        }
    }

    public class GivenMentionManyAfterFour : WhenMany
    {
        [Fact]
        public void ThenReturnFourAsMany()
        {
            Given<IMyRepository>().That(_ => _.List()).Returns(Many<MyModel>)
                .Using(Four<MyModel>).Then().Result.Has().Count(4);
            Specification.Is(
@"Using four MyModels
Given IMyRepository.List() returns many MyModels
When List()
Then Result has count 4");
        }
    }

    public class GivenMentionManyAfterOne : WhenMany
    {
        [Fact]
        public void ThenReturnThreeAsMany()
        {
            Given<IMyRepository>().That(_ => _.List()).Returns(Many<MyModel>)
                .Using(One<MyModel>).Then().Result.Has().Count(3);
            Specification.Is(
@"Using one MyModel
Given IMyRepository.List() returns many MyModels
When List()
Then Result has count 3");
        }
    }
}

public class WhenMockReturnsFewerElementsThanPreviouslyMentioned : Spec<MyRetriever, MyModel[]>
{
    public WhenMockReturnsFewerElementsThanPreviouslyMentioned()
        => When(_ => _.Create(An<int>()));

    [Fact]
    public void ThenItIsDifferentFromFirst()
    {
        Using(3)
            .Given<IMyRepository>().That(_ => _.Create(Three<MyModel>().Length))
            .Returns(Two<MyModel>)
            .Then().Result.Has().Count(2);
        Specification.Is(
@"Using 3
Given IMyRepository.Create(three MyModels' Length) returns two MyModels
When Create(an int)
Then Result has count 2");
    }
}