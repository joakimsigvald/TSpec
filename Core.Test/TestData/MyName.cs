namespace TSpec.Test.TestData;

public readonly struct MyName
{
    private const int _maxLength = 40;
    private readonly string _primitive;

    public string Primitive { get => _primitive; init => _primitive = Trim(value); }

    private static string Trim(string value) 
        => value?.Length >= _maxLength ? value[.._maxLength] : value ?? string.Empty;

    public static implicit operator string(MyName value) => value.Primitive;
    public static explicit operator MyName(string value) => new() { Primitive = value };
}