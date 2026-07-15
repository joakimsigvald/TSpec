namespace TSpec.Internal.Specification.ExpressionParsing.Expressions;

/// <summary>
/// A Mention factory occurrence in an expression tree.
/// <paramref name="Boundary"/> is the raw source of the outermost mention node
/// — the bare <c>Generic</c>, or the <c>Call</c> that supplied its constraints.
/// Anything in the outer expression's <c>Raw</c> past <c>Boundary</c> is
/// member-access drilldown (used by the describer to render <c>… 's Foo</c>).
/// </summary>
internal sealed record Mention(string Boundary, string Verb, string TypeArgs, IReadOnlyList<Expr>? Constraints);
