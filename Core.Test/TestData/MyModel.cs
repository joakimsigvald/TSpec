namespace TSpec.Test.TestData;

public record MyModel
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public IEnumerable<int> Values { get; set; } = null!;
}