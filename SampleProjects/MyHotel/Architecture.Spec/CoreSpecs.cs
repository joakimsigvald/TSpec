using TSpec.Architecture;
using TSpec.Architecture.CSharp;
using TSpec.Assert;

namespace MyHotel.Architecture.Spec;

public class CoreSpecs : TSpec.Spec
{
    [Fact]
    public void EachSpecSitsInTheFolderOfTheClassItSpecifies()
        => CoreSpecFiles.Has().All(file => CoreClassFolders.Contains(FolderOf(file)));

    [Fact]
    public void EachSpecFileDeclaresOneSpec()
        => CoreSpecFiles.Has().All(file => file.Types.Count == 1);

    private static IEnumerable<SourceFile> CoreSpecFiles
        => Source.Projects["Core.Spec"].Files.Where(file => file.Types.Any(IsSpec));

    // A spec derives from Spec, but a declared type has no base type to tell it by; MyHotel names every spec When….
    private static bool IsSpec(DeclaredType type) => type.Name.StartsWith("When", StringComparison.Ordinal);

    private static HashSet<string> CoreClassFolders
    {
        get
        {
            var core = Source.Projects["Core"];
            return [.. core.Files.SelectMany(file => file.Types).Select(type => FolderOf(type, core.AssemblyName))];
        }
    }

    // MyHotel.Core.Bookings.BookingService is specified in Bookings/BookingService.
    private static string FolderOf(DeclaredType type, string root)
        => string.Join('/', [.. type.Namespace[root.Length..].Split('.', StringSplitOptions.RemoveEmptyEntries), type.Name]);

    private static string FolderOf(SourceFile file) => string.Join('/', file.Path.Split('/')[..^1]);
}
