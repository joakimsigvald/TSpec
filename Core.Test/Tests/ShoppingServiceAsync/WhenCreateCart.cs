using TSpec.Assert;
using TSpec.Test.Subjects;

namespace TSpec.Test.Tests.ShoppingServiceAsync;

public abstract class WhenCreateCart : Spec<Subjects.ShoppingServiceAsync, ShoppingCart>
{
    protected int _id;

    protected WhenCreateCart() => When(_ => _.CreateCart(_id));

    public class GivenIdIsOne : WhenCreateCart
    {
        public GivenIdIsOne() => Using(() => _id = 1);

        [Fact]
        public void ThenCartIdIsOne()
        {
            Result.Id.Is(_id);
            Specification.Is(
                """
                Using _id = 1
                When _.CreateCart(_id)
                Then Result.Id is _id
                """);
        }
    }

    public class GivenIdIsTwo : WhenCreateCart
    {
        public GivenIdIsTwo() => Using(() => _id = 2);

        [Fact]
        public void ThenCartIdIsTwo()
        {
            Result.Id.Is(_id);
            Specification.Is(
                """
                Using _id = 2
                When _.CreateCart(_id)
                Then Result.Id is _id
                """);
        }
    }
}