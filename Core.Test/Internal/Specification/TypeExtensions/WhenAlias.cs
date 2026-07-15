using TSpec.Assert;
using TSpec.Internal.Specification;
using TSpec.Test.AutoFixture;
using TSpec.Test.Subjects.RecordStructDefaults;

namespace TSpec.Test.Internal.Specification.TypeExtensions;

public class WhenAlias : Spec<Type, string>
{
    public WhenAlias() => When(_ => _.Alias());

    [Fact] public void GivenString() => Using(typeof(string)).Then().Result.Is("string");
    [Fact] public void GivenMyModel() => Using(typeof(MyModel)).Then().Result.Is("MyModel");
    [Fact] public void GivenArrayOfMyModel() => Using(typeof(MyModel[])).Then().Result.Is("MyModel[]");
    [Fact] public void GivenArrayOfInt() => Using(typeof(int[])).Then().Result.Is("int[]");
    [Fact] public void GivenJaggedArrayOfInt() => Using(typeof(int[][])).Then().Result.Is("int[][]");
    [Fact] public void Given2DArrayOfInt() => Using(typeof(int[,])).Then().Result.Is("int[,]");
    [Fact] public void GivenListOfInt() => Using(typeof(List<int>)).Then().Result.Is("List<int>");
    [Fact] public void GivenIEnumerableOfInt() => Using(typeof(IEnumerable<int>)).Then().Result.Is("IEnumerable<int>");
    [Fact] public void GivenGenericClass() => Using(typeof(Moq.Mock<MyModel>)).Then().Result.Is("Mock<MyModel>");
    [Fact] public void GivenGenericInterface() => Using(typeof(Moq.IMock<MyModel>)).Then().Result.Is("IMock<MyModel>");
    [Fact] public void GivenTwoGenericParameters() => Using(typeof(Key<int, long>)).Then().Result.Is("Key<int, long>");
    [Fact] public void GivenNestedGenericParameters() => Using(typeof(Moq.Mock<Moq.IMock<MyModel>>)).Then().Result.Is("Mock<IMock<MyModel>>");
}