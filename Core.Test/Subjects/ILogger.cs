namespace TSpec.Test.Subjects;

public interface ILogger
{
    public ILogger ForContext(string name, object value);
    public void Information(string message);
    public void Warning(string message);
    public void Error(string message);
}