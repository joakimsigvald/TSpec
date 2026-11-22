using TSpec.Architecture.CSharp;

namespace MyHotel.Architecture.Spec;

/// <summary>The projects whose source the shape facts read: every project of the build, this one
/// too.</summary>
internal static class Sources
{
    public static IReadOnlyList<string> Names => [.. Source.Projects.Select(project => project.Name)];
}
