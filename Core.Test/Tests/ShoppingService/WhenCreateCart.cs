using TSpec.Assert;
using TSpec.Test.Subjects;

namespace TSpec.Test.Tests.ShoppingService;

public abstract class WhenCreateCart : ShoppingServiceSpec<ShoppingCart>
{
    protected int _id;

    protected WhenCreateCart() => When(_ => _.CreateCart(_id));

    public class GivenIdIsOne : WhenCreateCart
    {
        public GivenIdIsOne() => Using(() => _id = 1);

        [Fact]
        public void ThenCartIdIsOne()
        {
            Result.Id.Is(1);
            Specification.Is(
                """
                Using _id = 1
                When _.CreateCart(_id)
                Then Result.Id is 1
                """);
        }

        [Fact]
        public void ThenCartIdIsNotTwo()
        {
            Result.Id.Is().Not(2);
            Specification.Is(
                """
                Using _id = 1
                When _.CreateCart(_id)
                Then Result.Id is not 2
                """);
        }
    }

    public class GivenIdIsTwo : WhenCreateCart
    {
        public GivenIdIsTwo() => Using(() => _id = 2);

        [Fact]
        public void ThenCartIdIsTwo()
        {
            Result.Id.Is(2);
            Specification.Is(
                """
                Using _id = 2
                When _.CreateCart(_id)
                Then Result.Id is 2
                """);
        }
    }
}