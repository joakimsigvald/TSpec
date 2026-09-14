using TSpec.Assert;
using TSpec.Test.TestData;

namespace TSpec.Test.Given;

public abstract class WhenGivenArrayOfModelsAsync : Spec<MyService, MyModel[]>
{
    protected WhenGivenArrayOfModelsAsync() => When(_ => _.GetModelsAsync());

    public class GivenDefaultEnumerableProvided : WhenGivenArrayOfModelsAsync
    {
        public GivenDefaultEnumerableProvided()
            => Given<IMyRepository>().Returns(An<IEnumerable<MyModel>>);

        [Fact]
        public void ThenCanGetTaskOfEnumerable()
        {
            Then().Completes();
            Specification.Is(
                """
            Given IMyRepository returns an IEnumerable<MyModel>
            When GetModelsAsync()
            Then completes
            """);
        }
    }

    public class GivenThreeModels : WhenGivenArrayOfModelsAsync
    {
        public GivenThreeModels() => Given().Three<MyModel>();

        [Fact]
        public void ThenTheTaskHoldsThem() => Result.Is().EqualTo(Three<MyModel>());
    }

    public class GivenNoEnumerableProvided : WhenGivenArrayOfModelsAsync
    {
        [Fact]
        public void ThenTheTaskHoldsAGeneratedOne() => Result.Is().not.Null();
    }
}