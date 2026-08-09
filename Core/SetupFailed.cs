namespace TSpec;

/// <summary>
/// Exception thrown when executing a test with invalid setup
/// </summary>
public class SetupFailed : ApplicationException
{
    internal SetupFailed(string message) : base(message) { }

    internal SetupFailed(string message, Exception innerException)
        : base($"{message}, because: {innerException.Message}", innerException) { }

    /// <summary>
    /// Set as the failure leaves the pipeline that raised it, so a pipeline can tell a nested
    /// specification's setup failure — an outcome of its act, which it may report — from its own,
    /// which is the author's mistake and must escape.
    /// </summary>
    internal bool LeftItsPipeline { get; private set; }

    internal void MarkLeftItsPipeline() => LeftItsPipeline = true;
}