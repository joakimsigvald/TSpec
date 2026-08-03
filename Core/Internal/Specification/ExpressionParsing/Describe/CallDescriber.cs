using TSpec.Internal.Specification.ExpressionParsing.Expressions;

namespace TSpec.Internal.Specification.ExpressionParsing.Describe;

/// <summary>
/// Call-mode description (used by <c>DescribeCall</c>). Recognizes the
/// lambda-body shapes a mock setup or action expression typically takes
/// (<c>_ =&gt; _.Method(args)</c>, <c>_ =&gt; _.X = value</c>) and falls
/// back to value-mode for anything else. <see cref="_skipSubjectRef"/>
/// drops the leading <c>_.</c> when the caller (e.g. mock setup) prepends
/// the receiver name itself.
/// </summary>
internal sealed class CallDescriber(bool skipSubjectRef) : Describer
{
    private readonly bool _skipSubjectRef = skipSubjectRef;

    protected override string Render(Expr expr)
        => expr switch
        {
            Lambda l => DescribeLambda(l),
            New n => DescribeNew(n),
            Call c => $"{Path(c.Target)}{ArgList(c.Args)}",
            _ when DescribeMention(expr) is { } m => m,
            _ => Value.Describe(expr),
        };

    private string DescribeLambda(Lambda l)
        => l.Params.Count switch
        {
            0 => l.ToSource(),
            1 => DescribeOneArgLambda(l),
            _ when l.AsParamRefAssign() is { } pa2 => $"{pa2.Target.Name} {pa2.Op} {Value.Describe(pa2.Value)}",
            _ => l.ToSource()
        };

    private string DescribeOneArgLambda(Lambda l)
    {
        if (l.AsParamRefCall() is { } pc)
            return Prefixed(pc.Receiver, l.Params[0], pc.Target.Name, ArgList(pc.Args));
        if (l.AsParamRefAssign() is { } pa)
            return Prefixed(
                pa.Receiver, l.Params[0], pa.Target.Name, $" {pa.Op} {Value.Describe(pa.Value)}");
        if (_skipSubjectRef && l.Body is Unknown u && u.Raw.StartsWith(l.Params[0] + "."))
            return u.Raw[(l.Params[0].Length + 1)..];
        return Value.Describe(
            _skipSubjectRef ? SubjectElision.Elide(l.Body, l.Params[0]) : l.Body);
    }

    /// <summary>
    /// Drops the receiver only where it is the lambda's own parameter. <c>AsParamRefCall</c> accepts
    /// any receiver when the parameter is <c>_</c>, which is right for *matching* the shape but not
    /// for eliding: <c>_ =&gt; MyService.Echo(…)</c> calls a static class the specification has to
    /// keep naming, and <c>_</c> there is a subject the test never touches.
    /// </summary>
    private string Prefixed(Identifier receiver, string parameter, string memberName, string suffix)
        => _skipSubjectRef && receiver.Name == parameter
            ? $"{memberName}{suffix}"
            : $"{receiver.Name}.{memberName}{suffix}";
}