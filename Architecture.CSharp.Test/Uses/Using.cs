using static TSpec.Architecture.CSharp.Test.Uses.Constants;

namespace TSpec.Architecture.CSharp.Test.Uses;

/// <summary>
/// Uses the types beside it, by name only the first and the one of the package.
/// </summary>
internal static class Using
{
    internal static object[] All() => [new Constructed(), 2.Twice(), One, nameof(NamedInANameof), typeof(SourceFile)];
}
