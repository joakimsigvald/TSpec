using TSpec.Assert;
using Xunit.Sdk;

namespace TSpec.Test.Assert;

/// <summary>
/// A constructor that rejects the generated arguments is worked around silently, but the
/// workaround is reported under WARNINGS when an assertion later fails.
/// </summary>
public class WhenSetupWarning : Spec<Fussy>
{
    [Fact]
    public void GivenFallbackConstructor_ThenFailureListsWarning()
    {
        var ex = Xunit.Assert.Throws<XunitException>(() => A<Fussy>().Quantity.Is(1));
        ex.Message.Is("Expected A<Fussy>().Quantity to be 1 but found 0");
        ex.InnerException!.Message.Does().Contain("=== WARNINGS ===")
            .and.Contain("Fussy: the constructor rejected the generated arguments (ArgumentException)")
            .and.Contain("Using<Fussy>(...)");
    }

    [Fact]
    public void GivenNoWarning_ThenNoWarningSection()
    {
        var ex = Xunit.Assert.Throws<XunitException>(() => 1.Is(2));
        ex.InnerException!.Message.Does().not.Contain("=== WARNINGS ===");
    }
}

public class Fussy
{
    public Fussy() { }

    public Fussy(int quantity)
        => Quantity = quantity > 100
            ? quantity
            : throw new ArgumentException("Quantity must exceed 100");

    public int Quantity { get; private set; }
}
