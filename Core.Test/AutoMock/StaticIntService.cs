namespace TSpec.Test.AutoMock;

public class StaticIntService(int value)
{
    private readonly int _value = value;
    public int GetValue() => _value;
}