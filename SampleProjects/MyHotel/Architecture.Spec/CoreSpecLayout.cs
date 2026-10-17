using TSpec.Architecture;
using TSpec.Architecture.CSharp;
using TSpec.Assert;

namespace MyHotel.Architecture.Spec;

public class CoreSpecLayout : TSpec.Spec
{
    private static readonly SourceProjects _projects = Source.Projects.Under("MyHotel");

    [Fact]
    public void EachSpecSitsInTheFolderOfTheClassItSpecifies()
        => Specs.Where(file => !CoreClassFolders.Contains(FolderOf(file))).Is().Empty();

    [Fact]
    public void EachSpecFileDeclaresOnlyWhenClasses()
        => Specs.SelectMany(file => file.Types)
            .Where(type => !type.Name.StartsWith("When", StringComparison.Ordinal)).Is().Empty();

    // The files at the root, such as Global.cs, set the project up and specify nothing.
    private static IEnumerable<SourceFile> Specs
        => _projects["Core.Spec"].Files.Where(file => file.Path.Contains('/'));

    private static HashSet<string> CoreClassFolders
    {
        get
        {
            var core = _projects["Core"];
            return [.. core.Files.SelectMany(file => file.Types).Select(type => FolderOf(type, core.AssemblyName))];
        }
    }

    // MyHotel.Core.Bookings.BookingService is specified in Bookings/BookingService.
    private static string FolderOf(DeclaredType type, string root)
        => string.Join('/', [.. type.Namespace[root.Length..].Split('.', StringSplitOptions.RemoveEmptyEntries), type.Name]);

    private static string FolderOf(SourceFile file) => file.Path[..file.Path.LastIndexOf('/')];
}
