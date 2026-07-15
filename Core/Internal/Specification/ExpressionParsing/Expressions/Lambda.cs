namespace TSpec.Internal.Specification.ExpressionParsing.Expressions;

internal sealed record Lambda(string Raw, IReadOnlyList<string> Params, Expr Body) : Expr(Raw)
{
    public override IEnumerable<Expr> Children => [Body];

    /// Match <c>p =&gt; p.Prop = value</c> (any compound-assignment op).
    /// The first parameter must match the receiver (with <c>_</c> as wildcard).
    public ParamRefAssign? AsParamRefAssign() =>
        Body is Assign { Target: Member { Target: Identifier rcv } m } a && IsParamRef(rcv)
            ? new ParamRefAssign(rcv, m, a.Op, a.Value)
            : null;

    /// Match <c>p =&gt; p.Method(args)</c>.
    public ParamRefCall? AsParamRefCall() =>
        Body is Call { Target: Member { Target: Identifier rcv } m } c && IsParamRef(rcv)
            ? new ParamRefCall(rcv, m, c.Args)
            : null;

    private bool IsParamRef(Identifier id) =>
        Params.Count > 0 && (id.Name == Params[0] || Params[0] == "_");
}