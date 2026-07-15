namespace TSpec.Test.Subjects;

public class ShoppingService(
    IOrderService orderService,
    ILogger logger,
    (string shop, string division) names,
    int shopId)
{
    private readonly string _shopName = names.shop;
    private readonly string _division = names.division;

    public ShoppingCart CreateCart(int id) => new() { Id = id };

    public void PlaceOrder(ShoppingCart cart)
    {
        orderService.CreateOrder(cart);
        logger.ForContext("ShopId", shopId).ForContext("CartId", cart.Id).
            Information($"{_shopName}:{_division} created order");
    }
}