namespace TSpec.Test.AutoMock;

public class MyWrapper<TValue>(TValue use) 
{
    public (TValue, TValue) GetValues(TValue input) => (use, input);
}