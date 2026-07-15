using TSpec.Assert;
using TSpec.Test.Subjects;

namespace TSpec.Test.Tests.ShoppingServiceAsync;

public abstract class WhenCreateCart : Spec<Subjects.ShoppingServiceAsync, ShoppingCart>
{
    protected int Id;

    protected WhenCreateCart() => When(_ => _.CreateCart(Id));

    public class GivenIdIsOne : WhenCreateCart
    {
        public GivenIdIsOne() => Using(() => Id = 1);

        [Fact]
        public void ThenCartIdIsOne()
        {
            Result.Id.Is(Id);
            Specification.Is(
                """
                Using Id = 1
                When _.CreateCart(Id)
                Then Result.Id is Id
                """);
        }
    }

    public class GivenIdIsTwo : WhenCreateCart
    {
        public GivenIdIsTwo() => Using(() => Id = 2);

        [Fact]
        public void ThenCartIdIsTwo()
        {
            Result.Id.Is(Id);
            Specification.Is(
                """
                Using Id = 2
                When _.CreateCart(Id)
                Then Result.Id is Id
                """);
        }
    }
}