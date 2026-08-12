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
    internal const string FileName = "SPECIFICATION.md";

    private readonly PendingDocument _document;
    private readonly Assembly _specAssembly;

    /// <summary>
    /// Resolves the spec assembly, its subject and the output path. Throws
    /// <see cref="SetupFailed"/> if any of them cannot be determined.
    /// </summary>
    public SpecificationDocument()
    {
        _specAssembly = FindSpecAssembly();
        _document = PendingDocument.Prepare(ReadName(_specAssembly), AppContext.BaseDirectory);
        SpecificationCollector.IsActive = true;
    }

    /// <summary>
    /// Writes the document, but only when every non-skipped test in the assembly reported a pass.
    /// A filtered run, a failure, or a test whose constructor threw all leave requirements
    /// unreported, and publishing then would silently shorten the document — so the existing file
    /// is left alone and the missing requirements are named instead.
    /// </summary>
    public void Dispose()
    {
        SpecificationCollector.IsActive = false;
        var missing = SpecificationCollector.Missing(ExpectedRequirements.Of(_specAssembly));
        if (missing.Count == 0)
            _document.Write(SpecificationCollector.Entries);
        else
            Console.Error.WriteLine(Report(missing));
    }

    private static string Report(IReadOnlyCollection<string> missing)
        => $"TSpec: {FileName} left unchanged — {missing.Count} requirement(s) did not report a pass, "
        + "so the document would be incomplete. Run the whole suite green to regenerate it."
        + string.Concat(missing.Take(10).Select(requirement => $"\n  - {requirement}"))
        + (missing.Count > 10 ? $"\n  ... and {missing.Count - 10} more" : string.Empty);

    private static string ReadName(Assembly assembly)
        => assembly.GetName().Name
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
