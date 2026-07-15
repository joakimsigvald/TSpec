namespace TSpec.Test.Subjects.Shopping;

public interface IProductClient
{
    Task<Product[]> GetProducts(int? customerId, int? companyId, string[] partnumbers, int[] statusIds);
}