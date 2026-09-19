using TSpec.Assert;
using static TSpec.Times;

namespace TSpec.Test.AutoMock;

public class Meter
{
    public int Reading;

    public virtual void Read() { }
}

/// A name names a method; a property's has up to two accessors, so its name says not which was meant.
public class WhenVerifyingByName : Spec<MemberKindsService, string>
{
    [Fact]
    public void GivenAProperty_ThenItIsRefused()
        => RefusalOf(() => When(_ => _.GetName()).Then<IMemberKinds>(nameof(IMemberKinds.Name), Once))
            .Is("IMemberKinds.Name is a property; fields and properties cannot be verified by name");

    [Fact]
    public void GivenAField_ThenItIsRefused()
        => RefusalOf(() => When(_ => _.GetName()).Then<Meter>(nameof(Meter.Reading), Never))
            .Is("Meter.Reading is a field; fields and properties cannot be verified by name");

    [Fact]
    public void GivenANameOfNoMember_ThenItIsRefused()
        => RefusalOf(() => When(_ => _.GetName()).Then<IMemberKinds>("Nme", Never))
            .Is("IMemberKinds has no method Nme");

    [Fact]
    public void GivenAnAccessorsName_ThenItIsCounted()
        => When(_ => _.SetName("x")).Then<IMemberKinds>("set_Name", Once);

    private static string RefusalOf(Action verification)
        => Xunit.Assert.Throws<SetupFailed>(verification).Message;
}
