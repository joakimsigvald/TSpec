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
