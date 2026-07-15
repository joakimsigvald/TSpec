namespace TSpec.Test.TestData;

public readonly struct MyFullName
{
    private readonly (MyName first, MyName middle, MyName last) _primitive;

    public (MyName first, MyName middle, MyName last) Primitive
    { get => _primitive; init => _primitive = value; }

    public static implicit operator string(MyFullName value) => $"{value.Primitive.last}, {value.Primitive.first} {value.Primitive.middle}".Trim();
    public static explicit operator MyFullName((MyName, MyName, MyName) value) => new() { Primitive = value };
}