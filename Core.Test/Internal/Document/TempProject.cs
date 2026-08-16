namespace TSpec.Test.Internal.Document;

/// <summary>
/// A throwaway spec project on disk, shaped like a real one: a project file at the root and a
/// dependency manifest under bin/Debug/net10.0, which is where the fixture reads from.
/// </summary>
internal sealed class TempProject : IDisposable
{
    internal string Root { get; }
    internal string BaseDirectory { get; }

    internal TempProject(string assemblyName, string? depsJson = null, bool withProjectFile = true)
    {
        Root = Path.Combine(Path.GetTempPath(), $"tspec-{Guid.NewGuid():N}");
        BaseDirectory = Path.Combine(Root, "bin", "Debug", "net10.0");
        Directory.CreateDirectory(BaseDirectory);
        if (withProjectFile)
            File.WriteAllText(Path.Combine(Root, $"{assemblyName}.csproj"), "<Project />");
        if (depsJson is not null)
            File.WriteAllText(Path.Combine(BaseDirectory, $"{assemblyName}.deps.json"), depsJson);
    }

    /// <summary>
    /// Puts a real assembly and its debug information in the output directory under the name a
    /// subject would have, which is what lets the document identify the source it was built from.
    /// </summary>
    internal string CopyAssemblyAs(string name, System.Reflection.Assembly source)
    {
        var target = Path.Combine(BaseDirectory, $"{name}.dll");
        File.Copy(source.Location, target);
        File.Copy(Path.ChangeExtension(source.Location, ".pdb"), Path.ChangeExtension(target, ".pdb"));
        return target;
    }

    public void Dispose() => Directory.Delete(Root, recursive: true);
}
