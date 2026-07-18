using TSpec.Internal.Specification.ExpressionParsing.Expressions;

namespace TSpec.Internal.Specification.ExpressionParsing.Describe;

/// <summary>
/// Call-mode description (used by <c>ParseCall</c>). Recognizes the
/// lambda-body shapes a mock setup or action expression typically takes
/// (<c>_ =&gt; _.Method(args)</c>, <c>_ =&gt; _.X = value</c>) and falls
/// back to value-mode for anything else. <see cref="_skipSubjectRef"/>
/// drops the leading <c>_.</c> when the caller (e.g. mock setup) prepends
/// the receiver name itself.
/// </summary>
internal sealed class CallDescriber(bool skipSubjectRef) : Describer
{
    private readonly bool _skipSubjectRef = skipSubjectRef;

    public override string Describe(Expr expr)
        => expr switch
        {
            Lambda l => DescribeLambda(l),
            New n => DescribeNew(n),
            Call c => $"{c.Target.AsPath()}({DescribeAll(c.Args)})",
            _ when DescribeMention(expr) is { } m => m,
            _ => Value.Describe(expr),
        };

    private string DescribeLambda(Lambda l)
        => l.Params.Count switch
        {
            0 => l.Raw,
            1 => DescribeOneArgLambda(l),
            _ when l.AsParamRefAssign() is { } pa2 => $"{pa2.Target.Name} {pa2.Op} {Value.Describe(pa2.Value)}",
            _ => l.Raw
        };

    private string DescribeOneArgLambda(Lambda l)
    {
        if (l.AsParamRefCall() is { } pc)
            return Prefixed(pc.Receiver, pc.Target.Name, $"({DescribeAll(pc.Args)})");
        if (l.AsParamRefAssign() is { } pa)
            return Prefixed(pa.Receiver, pa.Target.Name, $" {pa.Op} {Value.Describe(pa.Value)}");
        if (_skipSubjectRef && l.Body is Unknown u && u.Raw.StartsWith(l.Params[0] + "."))
            return u.Raw[(l.Params[0].Length + 1)..];
        return Value.Describe(l.Body);
    }

    private string Prefixed(Identifier receiver, string memberName, string suffix) =>
        _skipSubjectRef ? $"{memberName}{suffix}" : $"{receiver.Name}.{memberName}{suffix}";
}