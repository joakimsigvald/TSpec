using System.Reflection;

namespace TSpec.Internal.Document;

/// <summary>
/// What the project under test says of itself: the Description of its project file, which the
/// build compiles into its assembly. Read from the assembly the spec project references — the one
/// sentence a project writes for its package heads its specification too, with nothing to
/// configure. A project that states none compiles no attribute, and there is nothing to read.
/// </summary>
internal static class SubjectDescription
{
    internal static string? Of(string assemblyName)
        => Reflowed(Find(assemblyName)?.GetCustomAttribute<AssemblyDescriptionAttribute>()?.Description);

    /// How the project file lays the Description out — on several lines, indented — is not part of it.
    internal static string? Reflowed(string? description)
        => string.IsNullOrWhiteSpace(description)
            ? null
            : string.Join(' ', description.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));

    /// <summary>
    /// Already loaded where a test has touched it; loaded by name otherwise, which the spec's
    /// direct reference lets the runtime resolve. Anything that stops it is not a reason to fail the
    /// run — the description is a courtesy, not a claim.
    /// </summary>
    private static Assembly? Find(string assemblyName)
    {
        var loaded = AppDomain.CurrentDomain.GetAssemblies()
            .FirstOrDefault(assembly => assembly.GetName().Name == assemblyName);
        if (loaded is not null)
            return loaded;
        try
        {
            return Assembly.Load(assemblyName);
        }
        catch (Exception exception) when (exception is IOException or BadImageFormatException)
        {
            return null;
        }
    }
}
