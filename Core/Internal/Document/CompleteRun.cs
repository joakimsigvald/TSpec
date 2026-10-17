using System.Reflection;

namespace TSpec.Internal.Document;

/// <summary>
/// What a document written from a run shares with the others: the spec assembly that opted in, and
/// the rule that only a run in which every test passed rewrites it.
/// </summary>
internal static class CompleteRun
{
    internal static Assembly SpecAssemblyDeclaring(Type fixture)
    {
        var candidates = AppDomain.CurrentDomain.GetAssemblies()
            .Where(assembly => Declares(assembly, fixture))
            .ToArray();
        return candidates.Length == 1
            ? candidates[0]
            : throw new SetupFailed(
                $"TSpec expected exactly one assembly declaring [assembly: AssemblyFixture(typeof({fixture.Name}))], "
                + $"but found {candidates.Length}.");
    }

    // The document is a by-product of the run and no test claims it, so where it cannot go nothing fails over it.
    internal static void Begin(string document, bool located)
    {
        if (located)
            SpecificationCollector.IsActive = true;
        else
            Console.Error.WriteLine(ProjectDirectory.Unlocatable(document, AppContext.BaseDirectory));
    }

    internal static void End(string document, Assembly specAssembly, Action write)
    {
        SpecificationCollector.IsActive = false;
        var missing = SpecificationCollector.Missing(ExpectedRequirements.Of(specAssembly));
        if (missing.Length == 0)
            write();
        else
            Console.Error.WriteLine(Report(document, missing));
    }

    private static string Report(string document, string[] missing)
        => $"TSpec: {document} left unchanged — {missing.Length} requirement(s) did not report a pass, "
        + "so the specification would be incomplete. Run the whole suite green to regenerate it."
        + string.Concat(missing.Take(10).Select(requirement => $"\n  - {requirement}"))
        + (missing.Length > 10 ? $"\n  ... and {missing.Length - 10} more" : string.Empty);

    private static bool Declares(Assembly assembly, Type fixture)
        => assembly.GetCustomAttributes<AssemblyFixtureAttribute>()
            .Any(attribute => attribute.AssemblyFixtureType == fixture);
}
