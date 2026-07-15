using TSpec.Assert;
using TSpec.Test.Subjects;

namespace TSpec.Test.Tests.ShoppingService;

public abstract class WhenCreateCart : ShoppingServiceSpec<ShoppingCart>
{
    protected int Id;

    protected WhenCreateCart() => When(_ => _.CreateCart(Id));

    public class GivenIdIsOne : WhenCreateCart
    {
        public GivenIdIsOne() => Using(() => Id = 1);

        [Fact]
        public void ThenCartIdIsOne()
        {
            Result.Id.Is(1);
            Specification.Is(
                """
                Using Id = 1
                When _.CreateCart(Id)
                Then Result.Id is 1
                """);
        }

        [Fact]
        public void ThenCartIdIsNotTwo()
        {
            Result.Id.Is().Not(2);
            Specification.Is(
                """
                Using Id = 1
                When _.CreateCart(Id)
                Then Result.Id is not 2
                """);
        }
    }

    public class GivenIdIsTwo : WhenCreateCart
    {
        public GivenIdIsTwo() => Using(() => Id = 2);

        [Fact]
        public void ThenCartIdIsTwo()
        {
            Result.Id.Is(2);
            Specification.Is(
                """
                Using Id = 2
                When _.CreateCart(Id)
                Then Result.Id is 2
                """);
        }
    }
}