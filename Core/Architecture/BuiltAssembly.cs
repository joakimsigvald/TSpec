using System.Reflection;
using System.Runtime.CompilerServices;

namespace TSpec.Architecture;

/// <summary>
/// The assembly the build made of a project, loaded by its name beside the running spec project.
/// </summary>
internal static class BuiltAssembly
{
    internal static IEnumerable<string> FriendsOf(string assemblyName)
        => Assembly.Load(assemblyName).GetCustomAttributes<InternalsVisibleToAttribute>()
            .Select(attribute => attribute.AssemblyName.Split(',')[0].Trim());
}
