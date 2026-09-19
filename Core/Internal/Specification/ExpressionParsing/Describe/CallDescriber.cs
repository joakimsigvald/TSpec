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
internal sealed class CallDescriber(bool skipSubjectRef, bool isMockCall = false) : Describer
{
    private readonly bool _skipSubjectRef = skipSubjectRef;

    protected override string Render(Expr expr)
        => expr switch
        {
            Lambda l when isMockCall => DescribeLambda(l with { Body = WithoutResult(l.Body) }),
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
        if (l.Body is Call { Target: Identifier { Name: "Set" }, Args: [var property, var value] })
            return DescribeSet(property, value, l.Params[0]);
        if (DescribeMockIndexer(l.Body, l.Params[0]) is { } indexer)
            return indexer;
        if (l.AsParamRefCall() is { } pc)
            return Prefixed(pc.Receiver, l.Params[0], pc.Target.Name, ArgList(pc.Args));
        if (l.AsParamRefAssign() is { } pa)
            return Prefixed(
                pa.Receiver, l.Params[0], pa.Target.Name, $" {pa.Op} {Value.Describe(pa.Value)}");
        if (_skipSubjectRef && l.Body is Call { Target: Identifier invoked } invocation && invoked.Name == l.Params[0])
            return ArgList(invocation.Args);
        if (_skipSubjectRef && l.Body is Unknown u && u.Raw.StartsWith(l.Params[0] + "."))
            return u.Raw[(l.Params[0].Length + 1)..];
        if (_skipSubjectRef && l.Body is Unknown unparsed && unparsed.Raw.StartsWith(l.Params[0] + "("))
            return unparsed.Raw[l.Params[0].Length..];
        return Value.Describe(
            _skipSubjectRef ? SubjectElision.Elide(l.Body, l.Params[0]) : l.Body);
    }

    /// A set of a mocked property is written Set(_.Name, value), since no expression can assign; it reads as the assignment.
    private string DescribeSet(Expr property, Expr value, string parameter)
        => $"{DescribeProperty(property, parameter)} = {Value.Describe(value)}";

    private string DescribeProperty(Expr property, string parameter)
        => DescribeMockIndexer(property, parameter)
        ?? Value.Describe(_skipSubjectRef ? SubjectElision.Elide(property, parameter) : property);

    /// An indexer on the mock reads as its index, which follows the mock's name as it is.
    private string? DescribeMockIndexer(Expr expr, string parameter)
        => isMockCall && expr is IndexExpr { Target: Identifier receiver } index && receiver.Name == parameter
            ? $"[{DescribeAll(index.Args)}]"
            : null;

    /// <summary>
    /// A mocked call chained through a task is written with Result, since no expression can await. The
    /// specification leaves it out of the chain, so the chain reads as the awaited call it stands for;
    /// arguments are left as written.
    /// </summary>
    private static Expr WithoutResult(Expr chain)
        => chain switch
        {
            Member { Name: "Result", Target: Call awaited } => WithoutResult(awaited),
            Member member => Retext(member with { Target = WithoutResult(member.Target) }),
            Call call => Retext(call with { Target = WithoutResult(call.Target) }),
            _ => chain,
        };

    private static Member Retext(Member member) => member with { Raw = member.ToSource() };

    private static Call Retext(Call call) => call with { Raw = call.ToSource() };

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