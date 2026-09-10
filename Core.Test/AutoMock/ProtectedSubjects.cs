namespace TSpec.Test.AutoMock;

/// <summary>
/// The shape of every HTTP adapter's handler: the member that decides the answer is protected, so
/// no lambda in a test can name it.
/// </summary>
public abstract class Dispatcher
{
    protected abstract Task<string> SendAsync(string request, int attempt);
    protected abstract void Record(string note);
    public Task<string> Dispatch(string request) => SendAsync(request, 1);
    public void Note(string note) => Record(note);
}

public class DispatchService(Dispatcher dispatcher)
{
    public Task<string> Send(string request) => dispatcher.Dispatch(request);
    public void Note(string note) => dispatcher.Note(note);
}
