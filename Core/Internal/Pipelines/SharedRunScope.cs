using System.ComponentModel;
using Xunit.v3;

namespace TSpec.Internal.Pipelines;

/// <summary>
/// xUnit makes one for each test class and disposes it after the class's last test, which is when
/// the class's shared run is torn down. Public only because every spec declares it as a class fixture.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public sealed class SharedRunScope : IDisposable
{
    private readonly Type? _specClass = (TestContext.Current.TestClass as IXunitTestClass)?.Class;

    /// <inheritdoc/>
    public void Dispose()
    {
        if (_specClass is not null)
            SharedRuns.End(_specClass);
    }
}
