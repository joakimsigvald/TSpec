namespace TSpec.Test.TestData;

public class MyValueTypeModel
{
    public MyValueTypeModel(MyValueInt intValue, MyZipCode zipCode, MyFullName name)
    {
        IntValue = intValue;
        ZipCode = zipCode;
        Name = name;
    }

    public MyValueInt IntValue { get; }
    public MyZipCode ZipCode { get; }
    public MyFullName Name { get; }
}
