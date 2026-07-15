namespace TSpec.Internal.Specification.ExpressionParsing.Expressions;

internal sealed record Unknown(string Raw) : Expr(Raw);