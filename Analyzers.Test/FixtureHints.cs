using System.Collections.Immutable;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace TSpec.Analyzers.Test;

internal static class FixtureHints
{
    /// The project's usings, which a sample compiled on its own does not get.
    private static readonly SyntaxTree _globalUsings = CSharpSyntaxTree.ParseText("""
        global using System;
        global using TSpec;
        global using TSpec.Assert;
        global using Xunit;
        """);

    private static readonly MetadataReference[] _references =
    [
        .. ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!)
            .Split(Path.PathSeparator)
            .Select(path => MetadataReference.CreateFromFile(path))
    ];

    private static readonly ImmutableArray<DiagnosticAnalyzer> _xunitAnalyzers =
    [
        .. Assembly.LoadFrom(Path.Combine(AppContext.BaseDirectory, "xunit.analyzers.dll")).GetTypes()
            .Where(type => !type.IsAbstract && type.GetCustomAttribute<DiagnosticAnalyzerAttribute>() is not null)
            .Select(type => (DiagnosticAnalyzer)Activator.CreateInstance(type)!)
    ];

    /// The messages of the xUnit1033 hints left standing on the compiled sample.
    internal static async Task<string[]> Shown(string sample, params DiagnosticSuppressor[] suppressors)
    {
        var compilation = CSharpCompilation.Create(
            "Sample", [_globalUsings, CSharpSyntaxTree.ParseText(sample)], _references,
            new(OutputKind.DynamicallyLinkedLibrary));
        AssertCompiles(compilation);
        var diagnostics = await compilation
            .WithAnalyzers([.. _xunitAnalyzers, .. suppressors], new CompilationWithAnalyzersOptions(
                new AnalyzerOptions([]), null, concurrentAnalysis: true, logAnalyzerExecutionTime: false,
                reportSuppressedDiagnostics: true))
            .GetAnalyzerDiagnosticsAsync();
        return [.. diagnostics.Where(it => it.Id == "xUnit1033" && !it.IsSuppressed).Select(it => it.GetMessage())];
    }

    private static void AssertCompiles(Compilation compilation)
    {
        var errors = compilation.GetDiagnostics()
            .Where(it => it.Severity == DiagnosticSeverity.Error)
            .Select(it => it.ToString())
            .ToArray();
        if (errors.Length > 0)
            throw new InvalidOperationException($"The sample does not compile: {string.Join("; ", errors)}");
    }
}
