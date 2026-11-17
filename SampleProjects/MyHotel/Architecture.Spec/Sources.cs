using TSpec.Architecture.CSharp;

namespace MyHotel.Architecture.Spec;

/// <summary>The projects whose source the shape facts read, one project at a time: every project of the
/// build, this one too.</summary>
internal static class Sources
{
    public static IReadOnlyList<string> Names => [.. Source.Projects.Select(project => project.Name)];
    public static TheoryData<string> Projects => [.. Names];

    /// <summary>A project's lines of code: the text of a string or a comment over several lines is
    /// none.</summary>
    public static IReadOnlyList<SourceLine> CodeLinesOf(string project) =>
        [.. Source.Projects[project].Files.SelectMany(file => file.SourceLines).Where(line => line.IsCode)];
}
