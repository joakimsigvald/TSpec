using System.Runtime.CompilerServices;
using TSpec.Assert;
using TSpec.Internal.Document;

namespace TSpec.Test.Internal.Document;

/// <summary>
/// Where a test class and a test method are written, read from the debug information the build
/// left beside the assembly. The expected file and line come from the compiler itself, through the
/// caller-info attributes, so the test states no path of its own.
/// </summary>
public class WhenLocateSource : Spec
{
    private static string _file = "";
    private static int _line;

    private static void Mark([CallerFilePath] string file = "", [CallerLineNumber] int line = 0)
        => (_file, _line) = (file, line);

    internal void Probe() => Mark();

    internal async Task ProbeAsync()
    {
        Mark();
        await Task.Yield();
    }

    private sealed class WithConstructor : Spec
    {
        public WithConstructor() => Mark();
    }

    private sealed class WithBlockConstructor : Spec
    {
        public WithBlockConstructor()
        {
            Mark();
        }
    }

    private sealed class WithoutConstructor : Spec
    {
        internal void Probe() => Mark();
    }

    [Fact]
    public void ThenAMethodIsWhereItsBodyStarts()
    {
        Probe();
        var location = SourceLocations.Of(typeof(WhenLocateSource).GetMethod(nameof(Probe), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!)!;
        location.File.Is(_file);
        location.Line.Is(_line);
    }

    /// <summary>
    /// An async method's body is compiled into a state machine, which is where its lines are kept.
    /// A block body starts at its brace, the line above the first statement.
    /// </summary>
    [Fact]
    public async Task GivenAnAsyncMethod_ThenItIsWhereItsBodyStarts()
    {
        await ProbeAsync();
        var location = SourceLocations.Of(typeof(WhenLocateSource).GetMethod(nameof(ProbeAsync), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!)!;
        location.File.Is(_file);
        location.Line.Is(_line - 1);
    }

    [Fact]
    public void ThenAClassIsWhereItsConstructorStarts()
    {
        using var _ = new WithConstructor();
        var location = SourceLocations.Of(typeof(WithConstructor))!;
        location.File.Is(_file);
        location.Line.Is(_line);
    }

    /// The constructor's own line, not the first statement in it, is where the class is found.
    [Fact]
    public void GivenABlockConstructor_ThenAClassIsWhereItIsDeclared()
    {
        using var _ = new WithBlockConstructor();
        var location = SourceLocations.Of(typeof(WithBlockConstructor))!;
        location.File.Is(_file);
        location.Line.Is(_line - 2);
    }

    /// A class that writes no constructor has no line of its own, but its file is known.
    [Fact]
    public void GivenNoConstructor_ThenAClassIsInTheFileOfItsMembers()
    {
        new WithoutConstructor().Probe();
        var location = SourceLocations.Of(typeof(WithoutConstructor))!;
        location.File.Is(_file);
        Xunit.Assert.Null(location.Line);
    }
}
