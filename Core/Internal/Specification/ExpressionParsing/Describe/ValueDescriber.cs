using TSpec.Internal.Specification.ExpressionParsing.Expressions;

namespace TSpec.Internal.Specification.ExpressionParsing.Describe;

/// <summary>
/// Value-mode description. Mention detection runs first; the switch below
/// covers every other shape. Recursion uses <see cref="Describe"/> directly
/// since values inside values stay in value mode.
/// </summary>
internal sealed class ValueDescriber : Describer
{
    protected override string Render(Expr expr)
    {
        if (DescribeMention(expr) is { } mention)
            return mention;

        return expr switch
        {
            Lambda l when l.Params.Count <= 2 && l.AsParamRefAssign() is { } pa
                => $"{pa.Target.Name} {pa.Op} {Describe(pa.Value)}",
            Lambda l when l.Params.Count <= 1 && l.AsParamRefWith() is { } w => DescribeAll(w.Init),
            Lambda l when l.Params.Count <= 1 => Describe(l.Body),
            Lambda l => l.ToSource(),
            Assign a => $"{AssignTargetName(a.Target)} {a.Op} {Describe(a.Value)}",
            // The copy is of something the reader was told about, so it is named: a value described
            // by its changes alone is not the value, and "BedCount = any int" is not a room.
            With w => $"{Describe(w.Target)} with {{ {DescribeAll(w.Init)} }}",
            TupleExpr t => $"({DescribeAll(t.Items)})",
            ArrayLit arr => $"[{DescribeAll(arr.Items)}]",
            Binary b => $"{Describe(b.Left)} {b.Op} {Describe(b.Right)}",
            Unary u => $"{u.Op}{Describe(u.Operand)}",
            Postfix p => $"{Describe(p.Operand)}{p.Op}",
            Conditional c => $"{Describe(c.Cond)} ? {Describe(c.Then)} : {Describe(c.Else)}",
            Cast c => $"({c.TypeName}){Describe(c.Operand)}",
            IsAs ia => $"{Describe(ia.Operand)} {ia.Op} {ia.TypeName}",
            InterpolatedString s => s.Quoted(Describe),
            Literal lit => lit.Quoted,
            New n => DescribeNew(n),
            Call c when c.IsDefaultOf() => $"default {Describe(c.Args[0])}",
            Call c when c.AsTagReference() is { } tag => $"the {tag.AsTagName()}",
            // A drilldown after a tag reads possessively — "the UpdatedRoom's RoomNumber" — which is
            // the rule a drilldown after a mention already follows.
            Member { Target: Call tagged } m when tagged.AsTagReference() is { } tag
                => $"the {tag.AsTagName()}'s {m.Name}",
            Call c when c.AsNaturalLanguageCall() is { } verb => $"{verb.AsWords()} {DescribeAll(c.Args)}",
            Call c => $"{c.Target.AsPath()}({DescribeAll(c.Args)})",
            NamedArg na => $"{na.Name}: {Describe(na.Value)}",
            Identifier id => id.Name,
            Unknown u => u.Raw,
            _ => expr.AsPath(),
        };
    }

    private static string AssignTargetName(Expr target) => target switch
    {
        Member m => m.Name,
        Identifier id => id.Name,
        _ => target.Raw,
    };
}