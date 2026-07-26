using System.Reflection;
using TSpec.Internal.Document;

namespace TSpec;

/// <summary>
/// Generates SPECIFICATION.md for a spec project. Enable it with one line in the spec project:
/// <code>[assembly: AssemblyFixture(typeof(SpecificationDocument))]</code>
/// The document is written to the spec project's source directory when the test run ends.
/// A project without that line behaves exactly as before.
/// </summary>
/// <remarks>
/// Everything the document depends on is resolved when the fixture is constructed, before any
/// test runs, so a misconfigured project fails at the start of the run rather than after it.
/// </remarks>
public sealed class SpecificationDocument : IDisposable
{
    internal const string _fileName = "SPECIFICATION.md";

    private readonly PendingDocument _document;

    /// <summary>
    /// Resolves the spec assembly, its subject and the output path. Throws
    /// <see cref="SetupFailed"/> if any of them cannot be determined.
    /// </summary>
    public SpecificationDocument()
        => _document = PendingDocument.Prepare(ReadSpecAssemblyName(), AppContext.BaseDirectory);

    /// <summary>
    /// Writes the document. Called by xunit after every test in the assembly has run.
    /// </summary>
    public void Dispose() => _document.Write();

    private static string ReadSpecAssemblyName()
        => FindSpecAssembly().GetName().Name
        ?? throw new SetupFailed("TSpec could not read the name of the spec assembly.");

    private static Assembly FindSpecAssembly()
    {
        var candidates = AppDomain.CurrentDomain.GetAssemblies()
            .Where(Declares)
            .ToArray();
        return candidates.Length == 1
            ? candidates[0]
            : throw new SetupFailed(
                $"TSpec expected exactly one assembly declaring [assembly: AssemblyFixture(typeof({nameof(SpecificationDocument)}))], "
                + $"but found {candidates.Length}.");
    }

    private static bool Declares(Assembly assembly)
        => assembly.GetCustomAttributes<AssemblyFixtureAttribute>()
            .Any(attribute => attribute.AssemblyFixtureType == typeof(SpecificationDocument));
}
