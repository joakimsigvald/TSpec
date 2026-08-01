namespace TSpec.Internal.Specification.ExpressionParsing.Expressions;

/// <summary>
/// Drops the subject-under-test parameter wherever it heads a member chain, so <c>_ =&gt; ++_.Counter</c>
/// describes as <c>++Counter</c>. The subject is already named once, beside the requirements as
/// <c>Subject under test:</c>, and naming it again in every clause repeats what position states —
/// the same rule that drops <c>When</c> under a <c>When …</c> heading.
/// </summary>
/// <remarks>
/// Everywhere, not only on the spine: <c>new(_.GetNextId(), _.GetConnectionString())</c> is the
/// subject doing two things, and a page that elided one and kept the other would read as though the
/// two came from different places.
/// <para>
/// A bare <c>_</c> that heads nothing is left alone — it names the subject as a value rather than
/// something it did, and eliding it would leave a hole where a word has to be.
/// </para>
/// <para>
/// <c>Raw</c> is recomputed for every rewritten node: describers fall back to it, and a stale one
/// would put the parameter back on the page.
/// </para>
/// </remarks>
internal static class SubjectElision
{
    internal static Expr Elide(Expr expr, string parameter)
    {
        Expr Rec(Expr e) => Elide(e, parameter);
        IReadOnlyList<Expr> RecAll(IReadOnlyList<Expr> es) => [.. es.Select(Rec)];

        return expr switch
        {
            Member { Target: Identifier id } m when id.Name == parameter => new Identifier(m.Name),
            Member m => Retext(m with { Target = Rec(m.Target) }),
            Call c => Retext(c with { Target = Rec(c.Target), Args = RecAll(c.Args) }),
            Unary u => Retext(u with { Operand = Rec(u.Operand) }),
            Postfix p => Retext(p with { Operand = Rec(p.Operand) }),
            Assign a => Retext(a with { Target = Rec(a.Target), Value = Rec(a.Value) }),
            Binary b => Retext(b with { Left = Rec(b.Left), Right = Rec(b.Right) }),
            New n => Retext(n with { Args = RecAll(n.Args), Init = n.Init is null ? null : RecAll(n.Init) }),
            With w => Retext(w with { Target = Rec(w.Target), Init = RecAll(w.Init) }),
            IndexExpr i => Retext(i with { Target = Rec(i.Target), Args = RecAll(i.Args) }),
            Conditional c => Retext(c with { Cond = Rec(c.Cond), Then = Rec(c.Then), Else = Rec(c.Else) }),
            TupleExpr t => Retext(t with { Items = RecAll(t.Items) }),
            ArrayLit a => Retext(a with { Items = RecAll(a.Items) }),
            Cast c => Retext(c with { Operand = Rec(c.Operand) }),
            NamedArg n => Retext(n with { Value = Rec(n.Value) }),
            Interpolation i => Retext(i with { Value = Rec(i.Value) }),
            _ => expr,
        };
    }

    private static Expr Retext(Member m) => m with { Raw = m.ToSource() };
    private static Expr Retext(Call c) => c with { Raw = c.ToSource() };
    private static Expr Retext(Unary u) => u with { Raw = u.ToSource() };
    private static Expr Retext(Postfix p) => p with { Raw = p.ToSource() };
    private static Expr Retext(Assign a) => a with { Raw = a.ToSource() };
    private static Expr Retext(Binary b) => b with { Raw = b.ToSource() };
    private static Expr Retext(New n) => n with { Raw = n.ToSource() };
    private static Expr Retext(With w) => w with { Raw = w.ToSource() };
    private static Expr Retext(IndexExpr i) => i with { Raw = i.ToSource() };
    private static Expr Retext(Conditional c) => c with { Raw = c.ToSource() };
    private static Expr Retext(TupleExpr t) => t with { Raw = t.ToSource() };
    private static Expr Retext(ArrayLit a) => a with { Raw = a.ToSource() };
    private static Expr Retext(Cast c) => c with { Raw = c.ToSource() };
    private static Expr Retext(NamedArg n) => n with { Raw = n.ToSource() };
    private static Expr Retext(Interpolation i) => i with { Raw = i.ToSource() };
}
