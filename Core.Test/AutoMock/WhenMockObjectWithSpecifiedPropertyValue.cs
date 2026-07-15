using TSpec.Assert;

namespace TSpec.Test.AutoMock;

public class WhenMockObjectWithSpecifiedPropertyValue : Spec<StaticObjectService, MyObject>
{
    public WhenMockObjectWithSpecifiedPropertyValue() => When(_ => _.GetValue());

    public class GivenItWasProvided : WhenMockObjectWithSpecifiedPropertyValue
    {
        [Fact]
        public void Then_It_Has_ProvidedValue()
        {
            Using(AFirst<MyObject>(_ => _.Age = 3)).Then().Result.Age.Is(3);
            Specification.Is(
                """
                Using a first MyObject { Age = 3 }
                When _.GetValue()
                Then Result.Age is 3
                """);
        }

        [Fact]
        public void Then_Can_Retrieve_The_Object()
        {
            Using(A<MyObject>(_ => _.Age = 3)).Then(The<MyObject>()).Age.Is(3);
            Specification.Is(
                """
                Using a MyObject { Age = 3 }
                When _.GetValue()
                Then the MyObject's Age is 3
                """);
        }
    }
}

public class StaticObjectService(MyObject value)
{
    private readonly MyObject _value = value;
    public MyObject GetValue() => _value;
}


public class MyObject
{
    public string Name { get; set; } = null!;
    public int Age { get; set; }
}