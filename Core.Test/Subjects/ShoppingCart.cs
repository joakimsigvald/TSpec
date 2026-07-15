namespace TSpec.Test.Subjects;

public record ShoppingCart
{
    private ShoppingCartItem[] _items = Array.Empty<ShoppingCartItem>();

    public bool IsOpen { get; set; }
    public int Id { get; set; }
    public ShoppingCartItem[] Items 
    { 
        get => _items; 
        set => _items = value.Select((it, i) => it with { LineNumber = i + 1}).ToArray(); }
}