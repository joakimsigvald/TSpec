using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace TSpec.Analyzers;

/// <summary>
/// Every spec declares SharedRunScope as a class fixture, so that xUnit tells TSpec when the class's
/// last test has run; no constructor needs to take it, and xUnit's hint that one should is noise on
/// every spec class. A hint about any other fixture is left alone.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class SharedRunScopeSuppressor : DiagnosticSuppressor
{
    private const string SharedRunScope = "TSpec.Internal.Pipelines.SharedRunScope";

    private static readonly SuppressionDescriptor _fixtureNotTaken = new(
        "TSPEC0001", "xUnit1033", "TSpec declares SharedRunScope on every spec, and no constructor needs to take it.");

    public override ImmutableArray<SuppressionDescriptor> SupportedSuppressions
        => ImmutableArray.Create(_fixtureNotTaken);

    public override void ReportSuppressions(SuppressionAnalysisContext context)
    {
        foreach (var diagnostic in context.ReportedDiagnostics.Where(IsAboutSharedRunScope))
            context.ReportSuppression(Suppression.Create(_fixtureNotTaken, diagnostic));
    }

    private static bool IsAboutSharedRunScope(Diagnostic diagnostic)
        => diagnostic.Properties.TryGetValue("TFixtureDisplayName", out var fixture) && fixture == SharedRunScope;
}
