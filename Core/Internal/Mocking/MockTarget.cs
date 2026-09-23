using TSpec.Internal.Specification;
using TSpec.Internal.TestData;

namespace TSpec.Internal.Mocking;

/// <summary>
/// What a setup is made on, or a verification counts: every mock of a type, or the one mock a mention
/// names. A mention is read when it is needed, so it is the value the pipeline arranged.
/// </summary>
internal sealed class MockTarget<TService> where TService : class
{
    private readonly Func<Context, TService>? _mention;

    private MockTarget(Func<Context, TService>? mention, string name)
    {
        _mention = mention;
        Name = name;
    }

    internal static MockTarget<TService> Family { get; } = new(null, typeof(TService).Alias());

    internal static MockTarget<TService> Of(Func<TService> mention, string mentionExpr)
        => new(_ => mention(), mentionExpr.Describe());

    internal static MockTarget<TService> Of(Tag<TService> tag, string tagExpr)
        => new(context => context.Mention(tag), $"the {tagExpr.AsTagName()}");

    /// How the specification names what is set up or verified.
    internal string Name { get; }

    internal bool IsFamily => _mention is null;

    internal IMocked In(Context context)
        => _mention is null
            ? context.GetMockFamily<TService>()
            : context.MockOf(_mention(context), Name);
}
