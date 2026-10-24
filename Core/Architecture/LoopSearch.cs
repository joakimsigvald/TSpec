using System.Diagnostics.CodeAnalysis;

namespace TSpec.Architecture;

/// <summary>
/// A walk of a graph's nodes in ordinal order, along their dependencies in ordinal order, that stops at the
/// first node it meets again while that node is still on the path that led there.
/// </summary>
internal sealed class LoopSearch(IReadOnlyDictionary<string, IReadOnlyList<string>> dependencies)
{
    private readonly List<string> _path = [];
    private readonly HashSet<string> _visited = [];

    internal bool TryFind([NotNullWhen(true)] out IReadOnlyList<string>? loop)
    {
        foreach (var node in dependencies.Keys)
            if (TryWalk(node, out loop))
                return true;

        loop = null;
        return false;
    }

    // A visited node that is no longer on the path has had every walk from it tried, so none closes a loop.
    private bool TryWalk(string node, [NotNullWhen(true)] out IReadOnlyList<string>? loop)
    {
        loop = _path.Contains(node) ? [.. _path.SkipWhile(on => on != node), node] : null;
        if (loop is not null)
            return true;

        return _visited.Add(node) && TryWalkOn(node, out loop);
    }

    private bool TryWalkOn(string node, [NotNullWhen(true)] out IReadOnlyList<string>? loop)
    {
        _path.Add(node);
        foreach (var next in dependencies.GetValueOrDefault(node, []))
            if (TryWalk(next, out loop))
                return true;

        _path.RemoveAt(_path.Count - 1);
        loop = null;
        return false;
    }
}
