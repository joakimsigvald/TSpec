namespace TSpec.Test.Subjects.Shopping;

public class BasketItemFactory
{
    private readonly IProductClient _productClient;

    public BasketItemFactory(IProductClient productClient) => _productClient = productClient;

    public async Task<BasketItem[]> CreateBasketItems(int customerId, int companyId, params NewBasketItem[] newItems)
    {
        var productsByPartNo = await GetProducts(customerId, companyId, newItems);
        return newItems.Select(bi => Merge(bi, productsByPartNo[bi.PartNo])).ToArray();
    }

    private static BasketItem Merge(NewBasketItem newItem, Product product)
        => new()
        {
            PartNo = newItem.PartNo,
            EanCode = product.EanCode,
            Name = product.Name,
        };

    private async Task<IDictionary<string, Product>> GetProducts(int customerId, int companyId, NewBasketItem[] basketItems)
    {
        var partNos = basketItems.Select(bi => bi.PartNo).Distinct().ToArray();
        var products = await _productClient.GetProducts(
            customerId, companyId, partNos, new[] { 1, 2, 3, 4 });
        if (products.Length < partNos.Length || products.Any(p => p == null))
            throw new BasketItemNotBuyable("");
        return products.ToDictionary(p => p.PartNo);
    }
}