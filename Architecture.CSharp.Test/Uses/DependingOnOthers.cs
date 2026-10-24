using Microsoft.CodeAnalysis.CSharp;

namespace TSpec.Architecture.CSharp.Test.Uses;

public static class DependingOnOthers
{
    public static object Of(SourceFile file) => (file, SyntaxKind.ClassKeyword, Source.Projects, DateTime.Now, new Constructed());
}
