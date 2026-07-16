using System.Runtime.ExceptionServices;
using TSpec.Internal.Pipelines;

namespace TSpec.Internal.TestData;

/// <summary>
/// Tracks disposable objects that TSpec instantiates for the subject-under-test graph,
/// so they can be disposed when the test pipeline is torn down.
/// Objects provided by the user (through Using, as value or factory) are never tracked,
/// nor are mocks or generated input data.
/// </summary>
internal class DisposalTracker
{
    private readonly List<object> _created = [];
    private bool _disposed;

    internal void Track(object? instance)
    {
        if (!_disposed && instance is IDisposable or IAsyncDisposable)
            _created.Add(instance);
    }

    /// <summary>
    /// Disposes all tracked objects in reverse creation order, so that the subject-under-test
    /// (created last) is disposed before the dependencies injected into it.
    /// All objects are disposed even if some throw; any exception is rethrown afterwards.
    /// </summary>
    internal void DisposeAll()
    {
        if (_disposed)
            return;

        _disposed = true;
        List<Exception>? failures = null;
        for (var i = _created.Count - 1; i >= 0; i--)
        {
            try
            {
                Dispose(_created[i]);
            }
            catch (Exception ex)
            {
                (failures ??= []).Add(ex);
            }
        }
        _created.Clear();
        if (failures is null)
            return;

        if (failures is [var single])
            ExceptionDispatchInfo.Capture(single).Throw();

        throw new AggregateException("Auto-dispose of subject-under-test graph failed", failures);
    }

    private static void Dispose(object instance)
    {
        if (instance is IDisposable disposable)
            disposable.Dispose();
        else if (instance is IAsyncDisposable asyncDisposable)
            AsyncHelper.Execute(() => asyncDisposable.DisposeAsync().AsTask());
    }
}
