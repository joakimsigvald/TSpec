namespace TSpec.Test.TestData;

public readonly struct MyName
{
    private const int MaxLength = 40;
    private readonly string _primitive;

    public string Primitive { get => _primitive; init => _primitive = Trim(value); }

    private static string Trim(string value) 
        => value?.Length >= MaxLength ? value[..MaxLength] : value ?? string.Empty;

    public static implicit operator string(MyName value) => value.Primitive;
    public static explicit operator MyName(string value) => new() { Primitive = value };
}