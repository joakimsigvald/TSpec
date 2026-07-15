namespace TSpec.Internal.Specification.ExpressionParsing.Expressions;

/// <c>p =&gt; p.Prop = value</c> shape (any compound-assignment op).
internal sealed record ParamRefAssign(Identifier Receiver, Member Target, string Op, Expr Value);