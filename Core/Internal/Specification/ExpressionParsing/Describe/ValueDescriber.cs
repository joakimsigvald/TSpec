using TSpec.Internal.Specification.ExpressionParsing.Expressions;
using TSpec.Internal.Specification.ExpressionParsing.Parse;

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
            With w => $"{Describe(w.Target)} with{Braced(w.Init)}",
            TupleExpr t => $"({Wrap.Enter}{DescribeAll(t.Items)}{Wrap.Exit})",
            ArrayLit arr => $"[{Wrap.Enter}{DescribeAll(arr.Items)}{Wrap.Exit}]",
            Binary b => $"{Operand(b, b.Left, onRight: false)} {b.Op} {Operand(b, b.Right, onRight: true)}",
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
            Call c => $"{Path(c.Target)}{ArgList(c.Args)}",
            NamedArg na => $"{na.Name}: {Describe(na.Value)}",
            Identifier id => id.Name,
            Unknown u => u.Raw,
            _ => Path(expr),
        };
    }

    /// <summary>
    /// An operand of a binary, parenthesized where the text would otherwise regroup it: a looser
    /// operand needs them, and so does an equally tight one on the right, since a run of one
    /// operator reads as nesting to the left.
    /// </summary>
    private string Operand(Binary parent, Expr operand, bool onRight)
    {
        var binding = BindingPower(operand);
        var parentBinding = BinaryRule.PrecedenceOf(parent.Op);
        return binding < parentBinding || onRight && binding == parentBinding
            ? $"({Describe(operand)})"
            : Describe(operand);
    }

    /// How tightly an operand holds together. Anything the parentheses cannot regroup binds tightest.
    private static int BindingPower(Expr expr) => expr switch
    {
        Assign or Conditional => BinaryRule.MinPrecedence - 1,
        IsAs isAs => BinaryRule.PrecedenceOf(isAs.Op),
        Binary b => BinaryRule.PrecedenceOf(b.Op),
        _ => int.MaxValue,
    };

    private static string AssignTargetName(Expr target) => target switch
    {
        Member m => m.Name,
        Identifier id => id.Name,
        _ => target.Raw,
    };
}