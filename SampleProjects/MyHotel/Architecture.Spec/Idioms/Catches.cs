using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TSpec.Architecture.CSharp;
using TSpec.Assert;

namespace MyHotel.Architecture.Spec.Idioms;

/// <summary>CODING-STANDARD-BACKEND.md 8a as an invariant over the syntax trees and what they bind to: a
/// catch names the types it handles, and a catch of `Exception` only rethrows, with any side effect in its
/// `when`. Whatever the filter answers, the original passes on - a filter that throws is dropped by the
/// runtime and counts as false. MyHotel has no construct that swallows all errors by design; the first one is
/// named here.</summary>
public class Catches : TSpec.Spec
{
    /// <summary>The one shape that reacts to every failure without keeping it.</summary>
    private const string Rethrowing = "catch (Exception e) when (Log(e)) { throw; }";

    private const string Instead =
        $"name the types, or only rethrow, with any side effect in the filter: {Rethrowing}";

    [Fact]
    public void NoCatchTakesEveryException() =>
        Source.Projects.Has().All(
            project => project.Nodes<CatchClauseSyntax>().Has().either.None(TakesEverything).or.Fix(Instead));

    [Theory]
    [InlineData("catch { }")]
    [InlineData("catch (Exception) { }")]
    [InlineData("catch (System.Exception e) { }")]
    [InlineData("catch (global::System.Exception e) { }")]
    [InlineData("catch (Exception e) when (e is TimeoutException) { }")]
    [InlineData("catch (Exception e) { if (cancelled) throw; }")]
    [InlineData("catch (Exception e) { Log(e); throw; }")]
    [InlineData("catch (Exception e) { throw Malformed(e); }")]
    [InlineData("catch (Error) { }")]
    public void ACatchTakesEveryException(string clause) => TakesEverything(CatchIn(clause)).Is().True();

    [Theory]
    [InlineData("catch (TimeoutException) { }")]
    [InlineData("catch (IOException e) when (e.HResult == 32) { }")]
    [InlineData("catch (Exception e) when (Log(e)) { throw; }")]
    [InlineData("catch (Exception e) when (Log(e) && false) { throw; }")]
    public void ACatchTakesOnlyTheTypesItNamesOrRethrows(string clause) =>
        TakesEverything(CatchIn(clause)).Is().False();

    private static bool TakesEverything(SourceNode<CatchClauseSyntax> clause) =>
        TakesAny(clause.Syntax.Declaration, clause.File.Model) && !Rethrows(clause.Syntax);

    /// <summary>A bare catch, or one of Exception however its type is written.</summary>
    private static bool TakesAny(CatchDeclarationSyntax? declaration, SemanticModel model) =>
        declaration is null
        || model.GetTypeInfo(declaration.Type).Type?.ToDisplayString() == "System.Exception";

    private static bool Rethrows(CatchClauseSyntax clause) =>
        clause.Block.Statements is [ThrowStatementSyntax { Expression: null }];

    /// <summary>The catch clause after an empty try in a snippet, where `Error` is an alias of
    /// Exception.</summary>
    private static SourceNode<CatchClauseSyntax> CatchIn(string clause) =>
        Source.Snippet($$"""
            using System;
            using System.IO;
            using Error = System.Exception;

            class C { void M() { try { } {{clause}} } }
            """).Nodes<CatchClauseSyntax>().Single();
}
