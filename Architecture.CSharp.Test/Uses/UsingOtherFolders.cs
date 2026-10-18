using TSpec.Architecture.CSharp.Internal;
using TSpec.Architecture.CSharp.Test.Declared;

namespace TSpec.Architecture.CSharp.Test.Uses;

/// <summary>
/// Uses a type of another folder, one at the project's root, which is in no folder, and one of the package's
/// Internal folder, which is not this project's.
/// </summary>
internal static class UsingOtherFolders
{
    internal static object[] All() => [new AClass(), typeof(DeclaredFiles), typeof(ProjectFolders)];
}
