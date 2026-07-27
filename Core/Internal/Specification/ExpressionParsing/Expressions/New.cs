namespace TSpec.Internal.Specification.ExpressionParsing.Expressions;

internal sealed record New(string Raw, string? TypeName, IReadOnlyList<Expr> Args, IReadOnlyList<Expr>? Init) : Expr(Raw)
{
    public override IEnumerable<Expr> Children => Init is null ? Args : Args.Concat(Init);

    /// An object initialiser with no constructor arguments drops the empty
    /// parentheses, matching how the expression is normally written.
    public override string ToSource()
    {
        string head = TypeName is null ? "new" : $"new {TypeName}";
        string args = Init is not null && Args.Count == 0 ? "" : $"({SourceList(Args)})";
        string init = Init is null ? "" : $" {{ {SourceList(Init)} }}";
        return head + args + init;
    }
}