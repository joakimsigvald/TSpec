using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TSpec.Architecture.CSharp;
using TSpec.Assert;

namespace MyHotel.Architecture.Spec.Idioms;

/// <summary>CODING-STANDARD-BACKEND.md 8a as an invariant over the syntax trees: a catch names the types it
/// handles, and a catch of `Exception` only rethrows, with any side effect in its `when`. Whatever the filter
/// answers, the original passes on - a filter that throws is dropped by the runtime and counts as false.
/// MyHotel has no construct that swallows all errors by design; the first one is named here. One fact per
/// project (Sources).</summary>
public class Catches : TSpec.Spec
{
    /// <summary>The one shape that reacts to every failure without keeping it.</summary>
    private const string Rethrowing = "catch (Exception e) when (Log(e)) { throw; }";

    private const string Instead =
        $"name the types, or only rethrow, with any side effect in the filter: {Rethrowing}";

    public static TheoryData<string> Projects => Sources.Projects;

    [Theory, MemberData(nameof(Projects))]
    public void NoCatchTakesEveryException(string project) =>
        Breach(project).Is().Null();

    [Theory]
    [InlineData("catch { }")]
    [InlineData("catch (Exception) { }")]
    [InlineData("catch (System.Exception e) { }")]
    [InlineData("catch (global::System.Exception e) { }")]
    [InlineData("catch (Exception e) when (e is TimeoutException) { }")]
    [InlineData("catch (Exception e) { if (cancelled) throw; }")]
    [InlineData("catch (Exception e) { Log(e); throw; }")]
    [InlineData("catch (Exception e) { throw Malformed(e); }")]
    public void ACatchTakesEveryException(string clause) => TakesEverything(CatchOf(clause)).Is().True();

    [Theory]
    [InlineData("catch (TimeoutException) { }")]
    [InlineData("catch (IOException e) when (e.HResult == 32) { }")]
    [InlineData("catch (Exception e) when (Log(e)) { throw; }")]
    [InlineData("catch (Exception e) when (Log(e) && false) { throw; }")]
    public void ACatchTakesOnlyTheTypesItNamesOrRethrows(string clause) =>
        TakesEverything(CatchOf(clause)).Is().False();

    private static string? Breach(string project) =>
        Sites(project) is [var first, ..] and var sites
            ? $"{sites.Count} catches take every exception, the first at {first}; {Instead}"
            : null;

    /// <summary>"file:line" for every catch in the project that takes every exception.</summary>
    private static IReadOnlyList<string> Sites(string project) =>
        [.. Source.Projects[project].Nodes<CatchClauseSyntax>()
            .Where(clause => TakesEverything(clause.Syntax))
            .Select(Reported)];

    private static bool TakesEverything(CatchClauseSyntax clause) =>
        TakesAny(clause.Declaration) && !Rethrows(clause);

    /// <summary>A bare catch, or `Exception` in each spelling but an alias, which syntax cannot see
    /// through.</summary>
    private static bool TakesAny(CatchDeclarationSyntax? declaration) =>
        declaration is null
        || declaration.Type.ToString() is "Exception" or "System.Exception" or "global::System.Exception";

    private static bool Rethrows(CatchClauseSyntax clause) =>
        clause.Block.Statements is [ThrowStatementSyntax { Expression: null }];

    private static string Reported(SourceNode<CatchClauseSyntax> clause) =>
        $"{clause.File}:{clause.File.Line(clause.Syntax)}";

    /// <summary>The catch clause of a snippet, after an empty try.</summary>
    private static CatchClauseSyntax CatchOf(string clause) =>
        ((TryStatementSyntax)SyntaxFactory.ParseStatement($"try {{ }} {clause}")).Catches[0];
}
