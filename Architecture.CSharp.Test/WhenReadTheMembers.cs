using Microsoft.CodeAnalysis;
using TSpec.Architecture;
using TSpec.Assert;
using TSpec.Test.Internal.Document;
using static TSpec.Architecture.CSharp.Test.WrittenFiles;

namespace TSpec.Architecture.CSharp.Test;

public class WhenReadTheMembers : Spec
{
    [Fact]
    public void ThenListEachMemberTheFileDeclaresInTheOrderItIsWritten()
        => MembersOf("""
            class A
            {
                int _count, _total;
                public A() { }
                public int Count => _count;
                public int this[int i] => i;
                public event System.Action? Changed;
                public static A operator +(A a, A b) => a;
                public static implicit operator int(A a) => a._count;
                ~A() { }
                void Run()
                {
                    int Twice(int x) => 2 * x;
                }
                delegate void Handler();
                enum Size { Small }
            }
            """).Select(member => (member.Name, member.Kind)).Is().EqualTo(
            [
                ("_count", MemberKind.Field), ("_total", MemberKind.Field), ("A", MemberKind.Constructor),
                ("Count", MemberKind.Property), ("this[]", MemberKind.Indexer), ("Changed", MemberKind.Event),
                ("operator +", MemberKind.Operator), ("implicit operator int", MemberKind.Conversion),
                ("~A", MemberKind.Destructor), ("Run", MemberKind.Method), ("Twice", MemberKind.LocalFunction),
                ("Handler", MemberKind.Delegate), ("Small", MemberKind.EnumMember),
            ]);

    [Fact]
    public void GivenARecordWithPositionalParameters_ThenItHasAConstructorAndAPropertyForEach()
        => MembersOf("record Room(int Number, string Name);").Select(member => (member.Name, member.Kind)).Is().EqualTo(
            [("Room", MemberKind.Constructor), ("Number", MemberKind.Property), ("Name", MemberKind.Property)]);

    [Fact]
    public void GivenAClassWithAPrimaryConstructor_ThenItsParametersAreNoProperties()
        => MembersOf("class Room(int number);").Select(member => (member.Name, member.Kind))
            .Is().EqualTo([("Room", MemberKind.Constructor)]);

    [Fact]
    public void ThenTellTheAccessibilityItIsDeclaredWith_OrHasByDefault()
        => MembersOf("""
            public class A
            {
                public void Open() { }
                protected internal void Shared() { }
                void Hidden() { }
            }
            public interface I
            {
                void Implied();
            }
            """).Select(member => (member.Name, member.Accessibility)).Is().EqualTo(
            [
                ("Open", Accessibility.Public), ("Shared", Accessibility.ProtectedOrInternal),
                ("Hidden", Accessibility.Private), ("Implied", Accessibility.Public),
            ]);

    [Fact]
    public void GivenATypeLessAccessibleThanItsMember_ThenTheMemberIsNoMoreAccessibleThanTheType()
        => MembersOf("""
            class Internal
            {
                public void Open() { }
                private class Private
                {
                    public void Hidden() { }
                }
                public class Nested
                {
                    protected void Derived() { }
                }
            }
            """).Select(member => (member.Name, member.Accessibility)).Is().EqualTo(
            [
                ("Open", Accessibility.Internal), ("Hidden", Accessibility.Private),
                ("Derived", Accessibility.ProtectedAndInternal),
            ]);

    [Fact]
    public void GivenAPrivateRecord_ThenItsConstructorAndPositionalPropertiesArePrivate()
        => MembersOf("public class A { private record Source(int Count); }")
            .Select(member => (member.Name, member.Accessibility))
            .Is().EqualTo([("Source", Accessibility.Private), ("Count", Accessibility.Private)]);

    [Fact]
    public void ThenGiveTheTypeItReturns()
        => MembersOf("class A { public (int, int) Pair(int x, string y) => (x, x); }").Single().Type?.ToString()
            .Is("(int, int)");

    [Fact]
    public void ThenGiveItsParameters()
        => MembersOf("class A { public (int, int) Pair(int x, string y) => (x, x); }").Single().Parameters
            .Select(parameter => parameter.ToString()).Is().EqualTo(["int x", "string y"]);

    [Fact]
    public void ThenGiveItsModifiers()
        => MembersOf("class A { public override string ToString() => \"\"; }").Single().Modifiers
            .Select(modifier => modifier.Text).Is().EqualTo(["public", "override"]);

    [Fact]
    public void ThenGiveItsAttributes()
        => MembersOf("class A { [Fact, Obsolete] void Run() { } }").Single().Attributes
            .Select(attribute => attribute.Name.ToString()).Is().EqualTo(["Fact", "Obsolete"]);

    [Fact]
    public void ThenGiveItsDeclaration()
        => MembersOf("class A { int _a, _b; }")[1].Syntax.ToString().Is("int _a, _b;");

    [Fact]
    public void ThenNameItByTheLineItsNameIsOn()
        => MembersOf("class A\n{\n    [Fact]\n    void Run() { }\n}\n").Single().ToString()
            .Is("Entry/Rooms/RoomEndpoints.cs:4 Run");

    [Fact]
    public void GivenAFailure_ThenNameEachMemberByItsFileLineAndName()
        => Xunit.Assert.Throws<Xunit.Sdk.XunitException>(() => MembersOf("class A { void Run() { } }").Is().Empty())
            .Message.Does().EndWith("[Entry/Rooms/RoomEndpoints.cs:1 Run]");

    [Fact]
    public void GivenAProject_ThenListTheMembersOfAllItsFiles()
        => Source.Projects["Test"].Members.Select(member => member.ToString())
            .Does().Contain("Test/Declared/APartial.More.cs:5 Second");

    private static IReadOnlyList<DeclaredMember> MembersOf(string code)
    {
        using var project = new TempProject("Entry");
        return Written(project, code).Members;
    }
}
