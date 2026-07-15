using TSpec.Assert;

namespace TSpec.Test.AutoFixture;

public abstract class WhenList : Spec<MyRetriever, MyModel[]>
{
    protected WhenList() 
        => When(_ => _.List())
        .Given<IMyRepository>().That(_ => _.List()).Returns(A<MyModel[]>);

    public class GivenListIsNull : WhenList 
    {
        public GivenListIsNull() => Given((MyModel[]?)null);
        [Fact] public void ThenReturnNull() => Result.Is().Null();
    }
}