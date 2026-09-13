namespace TSpec;

/// <summary>
/// How many times a mocked call is expected to have been invoked, given to <c>wasInvoked:</c>.
/// With <c>using static TSpec.Times;</c> it reads <c>Once</c>, <c>Never</c> or <c>AtLeast(2)</c>.
/// </summary>
public readonly struct Times
{
    private readonly int _from;
    private readonly int _to;

    private Times(int from, int to)
    {
        if (from < 0 || to < 0)
            throw new SetupFailed($"An invocation count cannot be negative, but {Math.Min(from, to)} was given");
        if (from > to)
            throw new SetupFailed($"No invocation count is at least {from} and at most {to}");
        _from = from;
        _to = to;
    }

    /// <summary>
    /// Exactly one invocation
    /// </summary>
    public static Times Once => new(1, 1);

    /// <summary>
    /// No invocation
    /// </summary>
    public static Times Never => new(0, 0);

    /// <summary>
    /// One invocation or more
    /// </summary>
    public static Times AtLeastOnce => new(1, int.MaxValue);

    /// <summary>
    /// No invocation, or one
    /// </summary>
    public static Times AtMostOnce => new(0, 1);

    /// <summary>
    /// Exactly the given number of invocations
    /// </summary>
    /// <param name="count">The number of invocations</param>
    public static Times Exactly(int count) => new(count, count);

    /// <summary>
    /// The given number of invocations or more
    /// </summary>
    /// <param name="count">The fewest invocations allowed</param>
    public static Times AtLeast(int count) => new(count, int.MaxValue);

    /// <summary>
    /// The given number of invocations or fewer
    /// </summary>
    /// <param name="count">The most invocations allowed</param>
    public static Times AtMost(int count) => new(0, count);

    /// <summary>
    /// A number of invocations from <paramref name="from"/> to <paramref name="to"/>, both included
    /// </summary>
    /// <param name="from">The fewest invocations allowed</param>
    /// <param name="to">The most invocations allowed</param>
    public static Times Between(int from, int to) => new(from, to);

    /// Moq checks an expression's count itself, and words its failure by the kind of count it was given.
    internal Moq.Times ToMoq() => (_from, _to) switch
    {
        (0, 0) => Moq.Times.Never(),
        (1, 1) => Moq.Times.Once(),
        (1, int.MaxValue) => Moq.Times.AtLeastOnce(),
        (0, 1) => Moq.Times.AtMostOnce(),
        (_, int.MaxValue) => Moq.Times.AtLeast(_from),
        (0, _) => Moq.Times.AtMost(_to),
        _ when _from == _to => Moq.Times.Exactly(_from),
        _ => Moq.Times.Between(_from, _to, Moq.Range.Inclusive),
    };
}
