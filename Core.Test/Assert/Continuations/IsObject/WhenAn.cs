using TSpec.Assert;

namespace TSpec.Test.Assert.Continuations.IsObject;

public class WhenAn : Spec
{
    [Fact]
    public void GivenMatchingType_ThenExposesTypedValue()
    {
        object one = new MyRecord("Ada");
        one.Is().An<MyRecord>().that.Name.Is("Ada");
        Specification.Is("One is an MyRecord that Name is \"Ada\"");
    }

    [Fact]
    public void GivenWrongType_ThenGetException()
    {
        object one = new MyOtherRecord("Ada");
        var ex = Xunit.Assert.Throws<Xunit.Sdk.XunitException>(() => one.Is().An<MyRecord>());
        ex.HasMessage(
            "Expected one to be an MyRecord but found MyOtherRecord { Name = Ada }",
            "One is an MyRecord");
    }
}
