using TSpec.Assert;
using TSpec.Test.Subjects;

namespace TSpec.Test.Pipeline;

public abstract class WhenValueTaskFunction : Spec<CounterService, int>
{
    protected WhenValueTaskFunction() => When(_ => _.IncrementAndGet("a"));

    public class GivenMockedCount : WhenValueTaskFunction
    {
        public GivenMockedCount() => Given<ICounterStore>().That(_ => _.GetCount("a")).Returns(() => 7);

        [Fact]
        public void ThenReturnTheMockedCount()
        {
            Then().Result.Is(7);
            Specification.Is(
                """
                Given ICounterStore.GetCount("a") returns 7
                When IncrementAndGet("a")
                Then Result is 7
                """);
        }
    }

    public class GivenNoMockedCount : WhenValueTaskFunction
    {
        [Fact]
        public void ThenReturnAGeneratedCount() => Then().Result.Is(1); //The auto-generated int, wrapped in a ValueTask
    }

    public class GivenSequenceOfCounts : WhenValueTaskFunction
    {
        public GivenSequenceOfCounts()
            => Given<ICounterStore>().That(_ => _.GetCount("a")).First().Returns(() => 1).AndNext().Returns(() => 2);

        [Fact]
        public void ThenReturnTheFirstCount()
        {
            Then().Result.Is(1);
            Specification.Is(
                """
                Given ICounterStore.GetCount("a") first returns 1
                  and next returns 2
                When IncrementAndGet("a")
                Then Result is 1
                """);
        }
    }

    public class GivenTheStoreThrows : WhenValueTaskFunction
    {
        public GivenTheStoreThrows() => Given<ICounterStore>().That(_ => _.GetCount("a")).Throws(An<ArgumentException>);

        [Fact]
        public void ThenThrow() => Then().Throws(The<ArgumentException>);
    }

    public class GivenTheStoreThrowsByType : WhenValueTaskFunction
    {
        public GivenTheStoreThrowsByType() => Given<ICounterStore>().That(_ => _.GetCount("a")).Throws<ArgumentException>();

        [Fact]
        public void ThenThrow() => Then().Throws<ArgumentException>();
    }

    public class GivenDefaultReturns : WhenValueTaskFunction
    {
        public GivenDefaultReturns() => Given<ICounterStore>().Returns(() => 4);

        [Fact]
        public void ThenReturnTheDefault() => Then().Result.Is(4);
    }

    public class GivenAsyncSetupAndTearDown : WhenValueTaskFunction
    {
        private int _offsetAfterTest = -1;

        public GivenAsyncSetupAndTearDown()
        {
            Given<ICounterStore>().That(_ => _.GetCount("a")).Returns(() => 7);
            Having(_ => SetOffset(_));
            Until(_ => RecordOffset(_));
        }

        private static async ValueTask SetOffset(CounterService service)
        {
            await Task.Yield();
            service.SetOffset(10);
        }

        private async ValueTask RecordOffset(CounterService service)
        {
            await Task.Yield();
            _offsetAfterTest = service.Offset;
        }

        [Fact]
        public void ThenTheAsyncSetupIsApplied()
        {
            Then().Result.Is(17);
            Specification.Is(
                """
                Given ICounterStore.GetCount("a") returns 7
                When IncrementAndGet("a")
                Having set offset _
                Until record offset _
                Then Result is 17
                """);
            _offsetAfterTest.Is(-1); //Teardown runs after the test method
        }
    }
}

public class WhenValueTaskAction : Spec<CounterService>
{
    public WhenValueTaskAction() => When(_ => _.Increment("a"));

    [Fact]
    public void ThenTheStoreIsIncremented()
    {
        Then<ICounterStore>(_ => _.Increment("a"));
        Specification.Is(
            """
            When Increment("a")
            Then ICounterStore.Increment("a")
            """);
    }
}

public class WhenValueTaskFunctionWithoutSubject : Spec<object, int>
{
    public WhenValueTaskFunctionWithoutSubject() => When(() => GetValue());

    private static async ValueTask<int> GetValue()
    {
        await Task.Yield();
        return 3;
    }

    [Fact]
    public void ThenReturnTheValue() => Then().Result.Is(3);
}

public class WhenValueTaskActionWithoutSubject : Spec
{
    private int _value;

    public WhenValueTaskActionWithoutSubject() => When(() => SetValue());

    private async ValueTask SetValue()
    {
        await Task.Yield();
        _value = 3;
    }

    [Fact]
    public void ThenTheValueIsSet()
    {
        Then();
        _value.Is(3);
    }
}

public class WhenValueTaskOfInterface : Spec<CounterService, int>
{
    public WhenValueTaskOfInterface() => When(_ => _.GetCartId(1));

    [Fact]
    public void ThenTheGeneratedCartIsReturned()
    {
        Given<ICounterStore>().That(_ => _.GetCart(1)).Returns(() => new ShoppingCart { Id = 5 });
        Then().Result.Is(5);
    }
}
