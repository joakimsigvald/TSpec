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

    internal static string[] Missing(Assembly specAssembly)
    {
        SpecificationCollector.IsActive = false;
        return SpecificationCollector.Missing(ExpectedRequirements.Of(specAssembly));
    }

    internal static string Report(string document, string[] missing)
        => $"TSpec: {document} left unchanged — {missing.Length} requirement(s) did not report a pass, "
        + "so the specification would be incomplete. Run the whole suite green to regenerate it."
        + string.Concat(missing.Take(10).Select(requirement => $"\n  - {requirement}"))
        + (missing.Length > 10 ? $"\n  ... and {missing.Length - 10} more" : string.Empty);

    private static bool Declares(Assembly assembly, Type fixture)
        => assembly.GetCustomAttributes<AssemblyFixtureAttribute>()
            .Any(attribute => attribute.AssemblyFixtureType == fixture);
}
