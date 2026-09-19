namespace TSpec.Internal.TestData;

internal sealed class SetupLambda
{
    // Per async flow: a mock's answer may run a setup lambda while the subject calls from another thread
    private readonly AsyncLocal<bool> _isRunning = new();

    internal bool IsRunning => _isRunning.Value;

    internal TValue Run<TValue>(Func<TValue> setup)
    {
        var wasRunning = _isRunning.Value;
        _isRunning.Value = true;
        try
        {
            return setup();
        }
        finally
        {
            _isRunning.Value = wasRunning;
        }
    }
}
