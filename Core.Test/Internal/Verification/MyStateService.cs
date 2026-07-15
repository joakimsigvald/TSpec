namespace TSpec.Test.Internal.Verification;

public class MyStateService 
{
    private int _state = 0;
    public int GetState() => _state;
    public void SetState(int state) => _state = state;
}