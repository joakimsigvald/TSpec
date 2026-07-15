namespace TSpec.Test.TestData;

public readonly struct MyValueInt
{
    public int Primitive { get; init; }
    public static implicit operator int(MyValueInt value) => value.Primitive;
    public static explicit operator MyValueInt(int value) => new() { Primitive = value };
}