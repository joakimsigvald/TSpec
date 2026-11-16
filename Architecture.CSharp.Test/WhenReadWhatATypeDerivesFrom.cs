using TSpec.Architecture;
using TSpec.Assert;

namespace TSpec.Architecture.CSharp.Test;

/// <summary>
/// The base types of the types in Signatures/Inheriting.cs, read from the build.
/// </summary>
public class WhenReadWhatATypeDerivesFrom : Spec
{
    [Fact]
    public void GivenItsBaseType_ThenItDerivesFromIt()
        => TypeNamed("Car").DerivesFrom(typeof(Signatures.Vehicle)).Is(true);

    [Fact]
    public void GivenABaseTypesOwnBase_ThenItDerivesFromItToo()
        => TypeNamed("Convertible").DerivesFrom(typeof(Signatures.Vehicle)).Is(true);

    [Fact]
    public void GivenATypeDerivedFromIt_ThenItDoesNotDeriveFromThat()
        => TypeNamed("Vehicle").DerivesFrom(typeof(Signatures.Car)).Is(false);

    [Fact]
    public void GivenTheTypeItself_ThenItDoesNotDeriveFromIt()
        => TypeNamed("Car").DerivesFrom(typeof(Signatures.Car)).Is(false);

    [Fact]
    public void GivenAFrameworkTypeUpTheChain_ThenItDerivesFromIt()
        => TypeNamed("Malfunction").DerivesFrom(typeof(Exception)).Is(true);

    [Fact]
    public void GivenAGenericBaseType_ThenItDerivesFromItWhateverItsTypeArguments()
        => TypeNamed("Wrapped").DerivesFrom(typeof(Signatures.Wrapper<>)).Is(true);

    [Fact]
    public void GivenAnInterfaceItImplements_ThenItDoesNotDeriveFromIt()
        => TypeIn("Signatures/Implementing.cs", "Room").DerivesFrom(typeof(Signatures.IRoom)).Is(false);

    [Fact]
    public void GivenAnEnum_ThenItDerivesFromEnum()
        => TypeIn("Declared/Kinds.cs", "AnEnum").DerivesFrom(typeof(Enum)).Is(true);

    [Fact]
    public void GivenABaseTypesName_ThenItDerivesFromIt()
        => TypeNamed("Convertible").DerivesFrom("Vehicle").Is(true);

    [Fact]
    public void GivenABaseTypesFullName_ThenItDerivesFromIt()
        => TypeNamed("Malfunction").DerivesFrom("System.InvalidOperationException").Is(true);

    [Fact]
    public void GivenAnotherTypesName_ThenItDoesNotDeriveFromIt()
        => TypeNamed("Car").DerivesFrom("Malfunction").Is(false);

    private static DeclaredType TypeNamed(string name)
        => TypeIn("Signatures/Inheriting.cs", name);

    private static DeclaredType TypeIn(string path, string name)
        => Source.Projects["Test"].Files.Single(file => file.Path == path)
            .Types.Single(type => type.Name == name);
}
