using TSpec.Architecture;
using TSpec.Architecture.CSharp;
using TSpec.Assert;

namespace MyHotel.Architecture.Spec;

public class CoreSpecs : TSpec.Spec
{
    [Fact]
    public void EachSpecSitsInTheFolderOfTheClassItSpecifies() =>
        CoreSpecFiles.Has().All(file => CoreClassFolders.Contains(FolderOf(file)));

    [Fact]
    public void EachSpecFileDeclaresOneSpec() =>
        CoreSpecFiles.Has().All(file => file.Types.Count == 1);

    private static IEnumerable<SourceFile> CoreSpecFiles =>
        Source.Projects["Core.Spec"].Files.Where(file => file.Types.Any(IsSpec));

    private static bool IsSpec(DeclaredType type) => type.DerivesFrom(typeof(TSpec.Spec<,>));

    private static IReadOnlySet<string> CoreClassFolders
    {
        get
        {
            var core = Source.Projects["Core"];
            HashSet<string> folders = [.. core.Files.SelectMany(file => file.Types)
                .Select(type => FolderOf(type, core.AssemblyName))];
            return folders;
        }
    }

    /// <summary>MyHotel.Core.Bookings.BookingService is specified in Bookings/BookingService.</summary>
    private static string FolderOf(DeclaredType type, string root) =>
        string.Join('/', [.. Segments(type.Namespace[root.Length..]), type.Name]);

    private static IEnumerable<string> Segments(string name) =>
        name.Split('.', StringSplitOptions.RemoveEmptyEntries);

    private static string FolderOf(SourceFile file) => string.Join('/', file.Path.Split('/')[..^1]);
}
