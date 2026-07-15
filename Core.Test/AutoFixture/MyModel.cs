namespace TSpec.Test.AutoFixture;

public record MyModel
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
}