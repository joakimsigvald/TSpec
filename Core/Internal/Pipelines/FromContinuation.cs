using TSpec.Continuations;
using TSpec.Internal.TestData.Generation;
using TSpec.Internal.TestData.Generation.Strategies;

namespace TSpec.Internal.Pipelines;

/// <summary>
/// The surface used by the StartingAt and Spaced extension methods to attach a source value sequence.
/// </summary>
internal interface ISequenceReceiver
{
    ISequence? Sequence { get; }
    void InstallSequence(ISequence sequence);
}

/// <summary>
/// The internal implementation of the From&lt;TSource&gt;() continuation.
/// Feeds the source value sequence configured with StartingAt and Spaced, if any,
/// to the type relay through the shared SequenceHolder.
/// </summary>
internal class FromContinuation<TSUT, TResult, TSource> :
    UsingTestPipeline<TSUT, TResult>,
    IUsingFromContinuation<TSUT, TResult, TSource>,
    ISequenceReceiver
{
    private readonly SequenceHolder _holder;

    internal FromContinuation(Spec<TSUT, TResult> parent, SequenceHolder holder) : base(parent)
        => _holder = holder;

    public ISequence? Sequence { get; private set; }

    public void InstallSequence(ISequence sequence)
    {
        Parent.Pipeline.SetSequence(_holder, sequence.Next);
        Sequence = sequence;
    }

    internal string DescribeSequence() => Sequence?.Describe() ?? string.Empty;
}
