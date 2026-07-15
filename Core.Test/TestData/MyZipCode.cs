namespace TSpec.Test.TestData;

public readonly struct MyZipCode
{
    private readonly uint _primitive;

    public uint Primitive { get => _primitive; init => _primitive = Trim(value); }

    private static uint Trim(uint value) => value % 100_000;

    public static implicit operator string(MyZipCode value) => $"{value.Primitive}".PadLeft(5, '0')[..5];
    public static explicit operator MyZipCode(uint value) => new() { Primitive = value };
}