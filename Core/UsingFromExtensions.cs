using TSpec.Internal.Pipelines;
using TSpec.Internal.TestData.Generation;

namespace TSpec;

/// <summary>
/// Sequence constraints for From&lt;TSource&gt;() with a numeric or temporal source type,
/// controlling which values the generator produces for the registered type relay.
/// Generated values are guaranteed to be unique: fixed numeric spacings wrap around at the type's
/// boundaries, and generation throws ValuesExhausted as soon as the sequence would repeat a value.
/// The overloads for each source type family are defined in their own partial class file.
/// </summary>
public static partial class UsingFromExtensions
{
    private static TSequence Sequence<TSequence>(object pipeline)
        where TSequence : class, ISequence, new()
    {
        var receiver = pipeline as ISequenceReceiver
            ?? throw new SetupFailed("Sequence constraints require a continuation created by From");
        var sequence = receiver.Sequence as TSequence ?? new();
        receiver.InstallSequence(sequence);
        return sequence;
    }
}
