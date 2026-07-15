namespace TSpec.Internal.Specification.ExpressionParsing.Expressions;

/// <c>p =&gt; p.Method(args)</c> shape.
internal sealed record ParamRefCall(Identifier Receiver, Member Target, IReadOnlyList<Expr> Args);