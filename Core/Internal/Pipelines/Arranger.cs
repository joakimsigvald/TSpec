namespace TSpec.Internal.Pipelines;

internal class Arranger
{
    private readonly List<Action> _usings = [];
    private readonly List<Action> _givens = [];

    internal void AppendUsing(Action arrangement) => _usings.Add(arrangement);
    internal void PrependGiven(Action arrangement) => _givens.Insert(0, arrangement);
    internal void AppendGiven(Action arrangement) => _givens.Add(arrangement);
    internal void Arrange() => _usings.Concat(_givens).ToList().ForEach(_ => _());
}