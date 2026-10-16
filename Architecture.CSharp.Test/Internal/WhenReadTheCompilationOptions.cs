using Microsoft.CodeAnalysis.CSharp;
using TSpec.Architecture.CSharp.Internal;
using TSpec.Assert;

namespace TSpec.Architecture.CSharp.Test.Internal;

/// <summary>
/// How the build parsed a project, as the compiler recorded it in the debug information.
/// </summary>
public class WhenReadTheCompilationOptions : Spec
{
    [Fact]
    public void ThenParseInTheLanguageVersionTheBuildUsed()
        => CompilationOptions.ParseOptions("Entry", new Dictionary<string, string> { ["language-version"] = "12.0" })
            .LanguageVersion.Is(LanguageVersion.CSharp12);

    [Fact]
    public void ThenParseWithTheSymbolsTheBuildDefined()
        => CompilationOptions.ParseOptions("Entry", new Dictionary<string, string> { ["define"] = "TRACE,DEBUG" })
            .PreprocessorSymbolNames.Is().EqualTo(["TRACE", "DEBUG"]);

    [Fact]
    public void GivenALanguageVersionTooNewToRead_ThenRefuse()
        => Xunit.Assert.Throws<SetupFailed>(
            () => CompilationOptions.ParseOptions("Entry", new Dictionary<string, string> { ["language-version"] = "99.0" }))
            .Message.Is("TSpec could not read the types of 'Entry', written in C# 99.0: "
                + "reference a newer Microsoft.CodeAnalysis.CSharp in the spec project.");
}
