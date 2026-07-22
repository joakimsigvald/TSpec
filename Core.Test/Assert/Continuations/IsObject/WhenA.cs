using TSpec.Assert;

namespace TSpec.Test.Assert.Continuations.IsObject;

public class WhenA : Spec
{
    [Fact]
    public void GivenMatchingType_ThenExposesTypedValue()
    {
        object one = new MyRecord("Ada");
        one.Is().A<MyRecord>().that.Name.Is("Ada");
        Specification.Is("One is a MyRecord that Name is \"Ada\"");
    }

    [Fact]
    public void GivenMatchingType_ThenReturnsTypedValueForUse()
    {
        object one = new MyRecord("Ada");
        MyRecord record = one.Is().A<MyRecord>().that;
        record.Name.Is("Ada");
    }

    [Fact]
    public void GivenSubtype_ThenDoesNotThrow()
    {
        object ex = new ApplicationException("boom");
        var exception = ex.Is().A<Exception>().that;
        exception.Message.Is("boom");
    }

    [Fact]
    public void GivenWrongType_ThenGetException()
    {
        object one = new MyOtherRecord("Ada");
        var ex = Xunit.Assert.Throws<Xunit.Sdk.XunitException>(() => one.Is().A<MyRecord>());
        ex.HasMessage(
            "Expected one to be a MyRecord but found MyOtherRecord { Name = Ada }",
            "One is a MyRecord");
    }

    [Fact]
    public void GivenInverted_ThenCannotAccessThat()
    {
        object one = new MyOtherRecord("Ada");
        Xunit.Assert.Throws<SetupFailed>(() => one.Is().not.A<MyRecord>().that);
    }
}