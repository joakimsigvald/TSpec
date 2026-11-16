using System.Runtime.CompilerServices;
using TSpec.Extensibility;

namespace TSpec.Architecture.CSharp;

/// <summary>
/// Fluent assertions on the dependencies between projects, and between the nodes of a graph
/// </summary>
public static class ArchitectureAssertions
{
    /// <summary>
    /// Get available assertions for the dependencies between projects
    /// </summary>
    /// <param name="actual">The dependencies to assert on</param>
    /// <param name="_">Ignore this parameter — it exists only to distinguish overloads</param>
    /// <param name="actualExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns>A continuation for making assertions on the dependencies</returns>
    public static IsProjectGraph Is(
        this SourceProjects? actual,
        Ignore _ = default,
        [CallerArgumentExpression(nameof(actual))] string? actualExpr = null)
        => Constraints.Create<SourceProjects, IsProjectGraph>(actual, actualExpr!);

    /// <summary>
    /// Get available assertions for the dependencies between the nodes of a graph
    /// </summary>
    /// <param name="actual">The dependencies to assert on</param>
    /// <param name="_">Ignore this parameter — it exists only to distinguish overloads</param>
    /// <param name="actualExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns>A continuation for making assertions on the dependencies</returns>
    public static IsDependencyGraph Is(
        this DependencyGraph? actual,
        Ignore _ = default,
        [CallerArgumentExpression(nameof(actual))] string? actualExpr = null)
        => Constraints.Create<DependencyGraph, IsDependencyGraph>(actual, actualExpr!);
}
