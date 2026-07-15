namespace TSpec.Test.Subjects.Shopping;

public class Basket
{
    public int Id { get; set; }
    public BasketItem[] Items { get; internal set; } = null!;
    public int CustomerId { get; set; }
    public int CompanyId { get; set; }
}