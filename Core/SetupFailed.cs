namespace TSpec;

/// <summary>
/// Exception thrown when executing a test with invalid setup
/// </summary>
public class SetupFailed : ApplicationException
{
    internal SetupFailed(string message) : base(message) { }

    internal SetupFailed(string message, Exception innerException)
        : base($"{message}, because: {innerException.Message}", innerException) { }
}