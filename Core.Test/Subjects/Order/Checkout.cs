using TSpec.Test.Subjects.Shopping;

namespace TSpec.Test.Subjects.Order;

public class Checkout
{
    public Basket Basket { get; set; } = null!;
    public bool IsOpen { get; set; }
}