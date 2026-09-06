namespace TSpec.Test.Tests.Delay;

public class DelayedState(int delayMs)
{
    private int _previousState;
    private DateTime _latestUpdate = DateTime.MinValue;
    private int _currentState;

    /// <summary>
    /// What the last read of <see cref="State"/> measured since the state was set. Exposed so a
    /// test can tell a stalled machine from a wrong answer — see <see cref="Stall"/>.
    /// </summary>
    public double LastElapsedMs { get; private set; }

    public int State
    {
        get
        {
            var elapsedTime = DateTime.Now - _latestUpdate;
            LastElapsedMs = elapsedTime.TotalMilliseconds;
            return elapsedTime.TotalMilliseconds < delayMs
                ? _previousState : _currentState;
        }
    }

    public void SetState(int newState)
    {
        _latestUpdate = DateTime.Now;
        _previousState = _currentState;
        _currentState = newState;
    }
}

/// <summary>
/// Skips the test when a busy machine stretched the wait past the delay it was meant to stay
/// inside. Tests that wait longer than the delay don't need this — a wait can overrun, but never
/// finishes early.
/// </summary>
internal static class Stall
{
    internal static void SkipIfPast(DelayedState subject, int delayMs, int waitMs)
    {
        if (subject.LastElapsedMs < delayMs)
            return;
        Xunit.Assert.Skip(
            $"The machine stalled: {subject.LastElapsedMs:F0} ms passed for a {waitMs} ms wait, "
            + $"which is past the {delayMs} ms delay this test needs to stay inside.");
    }
}