namespace TSpec.Internal.Pipelines;

internal class Arranger
{
    private readonly List<Action> _usings = [];
    private readonly List<Action> _values = [];
    private readonly List<Action> _mocks = [];

    internal void AppendUsing(Action arrangement) => _usings.Add(arrangement);
    internal void PrependGiven(Action arrangement) => _values.Insert(0, arrangement);
    internal void AppendGiven(Action arrangement) => _mocks.Add(arrangement);
    internal void Arrange() => _usings.Concat(_values).ToList().ForEach(_ => _());
    internal void Mock() => _mocks.ForEach(_ => _());
}
