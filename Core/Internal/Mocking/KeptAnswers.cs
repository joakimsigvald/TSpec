namespace TSpec.Internal.Mocking;

/// <summary>
/// What a mock has answered, kept one per address, so a call no setup matches is answered the same
/// however often it is made. A property is the no-argument case: a set writes what a read takes.
/// </summary>
internal sealed class KeptAnswers
{
    private readonly List<KeptAnswer> _kept = [];

    internal bool TryRead(CallAddress address, out object? value)
    {
        lock (_kept)
        {
            var kept = At(address);
            value = kept?.Value;
            return kept is not null;
        }
    }

    internal void Write(CallAddress address, object? value)
    {
        lock (_kept)
        {
            if (At(address) is { } kept)
                kept.Value = value;
            else
                _kept.Add(new(CallMatcher.Exactly(address.Member, address.Arguments), value));
        }
    }

    private KeptAnswer? At(CallAddress address)
        => _kept.FirstOrDefault(kept => kept.Address.Matches(address.Member, address.Arguments));

    private sealed class KeptAnswer(CallMatcher address, object? value)
    {
        internal CallMatcher Address => address;

        internal object? Value { get; set; } = value;
    }
}
