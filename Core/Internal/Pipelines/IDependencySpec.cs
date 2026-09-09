namespace TSpec.Internal.Pipelines;

/// <summary>
/// A spec TSpec built into another spec's subject graph. It is torn down with that graph, and what
/// it claims is the enclosing test's business — that test asserts on its result and specification.
/// </summary>
internal interface IDependencySpec
{
    void DisposeAsDependency();
}
