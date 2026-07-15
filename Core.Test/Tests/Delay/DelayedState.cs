namespace TSpec.Test.Tests.Delay;

public class DelayedState(int delayMs)
{
    private int _previousState;
    private DateTime _latestUpdate = DateTime.MinValue;
    private int _currentState;
    public int State
    {
        get
        {
            var elapsedTime = DateTime.Now - _latestUpdate;
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