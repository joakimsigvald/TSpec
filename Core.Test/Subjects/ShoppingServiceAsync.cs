namespace TSpec.Test.Subjects;

public class ShoppingServiceAsync
{
    private readonly IOrderService _orderService;
    private readonly IShoppingCartRepository _repository;

    public ShoppingServiceAsync(IOrderService orderService, IShoppingCartRepository repository)
        => (_orderService, _repository) = (orderService, repository);

    public Task<ShoppingCart> CreateCart(int id)
        => Task.FromResult(new ShoppingCart { Id = id });

    public async Task<ShoppingCart> AddToCart(int cartId, ShoppingCartItem item)
    {
        var cart = await _repository.GetCart(cartId);
        ShoppingCart res = new() 
        {
            Id = cart.Id,
            Items = cart.Items.Append(item).ToArray()
        };
        await _repository.StoreCart(res);
        return res;
    }

    public Task PlaceOrder(ShoppingCart cart)
        => cart.IsOpen ? Task.Run(() => _orderService.CreateOrder(cart)) : throw new NotPurchasable();

    public async Task<ShoppingCart> RemoveFromCart(int cartId, ShoppingCartItem item)
    {
        var cart = await _repository.GetCart(cartId);
        ShoppingCart res = new() 
        {
            Id = cart.Id,
            Items = cart.Items.Except(new[] { item}).ToArray()
        };
        await _repository.StoreCart(res);
        return res;
    }
}