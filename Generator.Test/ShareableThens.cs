using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace TSpec.Generator.Test;

internal static class ShareableThens
{
    /// The project's implicit usings, which a sample compiled on its own does not get.
    private static readonly SyntaxTree _globalUsings = CSharpSyntaxTree.ParseText("""
        global using System.Collections.Generic;
        global using System.Threading.Tasks;
        global using TSpec.Assert;
        global using Xunit;
        """);

    private static readonly MetadataReference[] _references =
    [
        .. ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!)
            .Split(Path.PathSeparator)
            .Select(path => MetadataReference.CreateFromFile(path))
    ];

    /// The methods the generator lists, read back from the attributes it adds to the compiled sample.
    internal static string[] In(string sample)
    {
        var compilation = CSharpCompilation.Create(
            sample, [_globalUsings, CSharpSyntaxTree.ParseText(SourceOf(sample))], _references,
            new(OutputKind.DynamicallyLinkedLibrary));
        AssertCompiles(compilation, "The sample");
        CSharpGeneratorDriver.Create(new ShareableThenGenerator())
            .RunGeneratorsAndUpdateCompilation(compilation, out var generated, out _);
        AssertCompiles(generated, "The generated code");
        return [.. generated.Assembly.GetAttributes()
            .Where(it => it.AttributeClass?.ToDisplayString() == typeof(ShareableThenAttribute).FullName)
            .Select(it => (string)it.ConstructorArguments[1].Value!)];
    }

    private static string SourceOf(string sample)
    {
        using var source = typeof(ShareableThens).Assembly.GetManifestResourceStream(sample)!;
        return new StreamReader(source).ReadToEnd();
    }

    private static void AssertCompiles(Compilation compilation, string what)
    {
        var errors = compilation.GetDiagnostics()
            .Where(it => it.Severity == DiagnosticSeverity.Error)
            .Select(it => it.ToString())
            .ToArray();
        if (errors.Length > 0)
            throw new InvalidOperationException($"{what} does not compile: {string.Join("; ", errors)}");
    }
}
