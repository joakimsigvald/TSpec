using TSpec.Assert;
using TSpec.Internal.Document;

namespace TSpec.Test.Internal.Document;

/// <summary>
/// The id the document header carries. It names the source the subject was built from, so a
/// document generated before an implementation change is distinguishable from one generated after.
/// Everything else has to leave it alone — which machine built it, which directory it was built in,
/// and which commit was checked out. The last of those is why the files the build generates into
/// obj/ are excluded: one of them carries the commit id, so counting it moved the document's id on
/// every commit whether or not a line of the implementation had changed.
/// </summary>
public class WhenDigestSubjectSource : Spec
{
    private static readonly SourceFile[] _implementation =
    [
        new(@"C:\src\MyHotel\Core\Rooms\RoomService.cs", "1a2b3c"),
        new(@"C:\src\MyHotel\Core\Rooms\IRoomStore.cs", "4d5e6f")
    ];

    private static SourceFile AssemblyInfoAt(string commit)
        => new(@"C:\src\MyHotel\Core\obj\Debug\net10.0\Core.AssemblyInfo.cs", commit);

    private static string Digest(params SourceFile[] files) => SourceDigest.Of(files);

    /// <summary>
    /// The same implementation built at two commits states one id. This is the whole point: the
    /// only thing that differed between two builds of unchanged source was the commit the SDK
    /// wrote into the generated assembly info.
    /// </summary>
    [Fact]
    public void ThenIgnoreWhatTheBuildGeneratedIntoObj()
        => Digest([.. _implementation, AssemblyInfoAt("89837352")])
            .Is(Digest([.. _implementation, AssemblyInfoAt("191c2bc7")]));

    /// <summary>
    /// Absolute paths, so two developers building the same commit would otherwise disagree.
    /// </summary>
    [Fact]
    public void ThenIgnoreWhereTheProjectWasBuilt()
        => Digest([.. _implementation])
            .Is(Digest([
                new(@"/home/dev/MyHotel/Core/Rooms/RoomService.cs", "1a2b3c"),
                new(@"/home/dev/MyHotel/Core/Rooms/IRoomStore.cs", "4d5e6f")]));

    /// <summary>The compiler's ordering is an implementation detail of the compiler.</summary>
    [Fact]
    public void ThenIgnoreTheOrderTheCompilerListedThemIn()
        => Digest([.. _implementation])
            .Is(Digest([_implementation[1], _implementation[0]]));

    /// <summary>
    /// The requirement the id exists for: change the implementation without re-running the suite,
    /// and the document still carries the id of the source it was actually generated from.
    /// </summary>
    [Fact]
    public void ThenMoveWhenAnImplementationFileChanges()
        => Digest([.. _implementation])
            .Is().Not(Digest([new(_implementation[0].Path, "999999"), _implementation[1]]));

    [Fact]
    public void ThenMoveWhenAFileIsAdded()
        => Digest([.. _implementation])
            .Is().Not(Digest([.. _implementation, new(@"C:\src\MyHotel\Core\Rooms\Room.cs", "7a8b9c")]));

    /// <summary>Eight hex characters, the shape the header has always rendered.</summary>
    [Fact]
    public void ThenStateEightHexCharacters()
        => Digest([.. _implementation]).Does().Match("^[0-9a-f]{8}$");

    /// <summary>
    /// Against a real build. The compiler records a checksum per source file in the PDB, and that
    /// is where the digest comes from — no reading of the developer's source tree.
    /// </summary>
    [Fact]
    public void GivenARealAssembly_ThenReadWhatTheCompilerRecorded()
        => SourceDigest.Read(typeof(Spec).Assembly.Location)
            .Has().Some(file => file.Path.EndsWith("SourceDigest.cs", StringComparison.Ordinal));

    /// <summary>
    /// The exclusion is not hypothetical: a real build does put generated files in the PDB.
    /// </summary>
    [Fact]
    public void GivenARealAssembly_ThenTheGeneratedFilesAreThereToExclude()
        => SourceDigest.Read(typeof(Spec).Assembly.Location)
            .Has().Some(file => file.Path.Contains(@"\obj\", StringComparison.OrdinalIgnoreCase)
                || file.Path.Contains("/obj/", StringComparison.OrdinalIgnoreCase));

    [Fact]
    public void GivenARealAssembly_ThenStateAnIdForIt()
        => SourceDigest.Of(typeof(Spec).Assembly.Location).Does().Match("^[0-9a-f]{8}$");

    /// <summary>Debug information can be turned off, and then there is no source to state.</summary>
    [Fact]
    public void GivenNoDebugInformation_ThenStateNothing()
        => SourceDigest.Of(Path.Combine(Path.GetTempPath(), "no-such-assembly.dll")).Is().Null();
}
