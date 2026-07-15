using System.Text;

namespace TSpec.Internal.Specification;

internal class SpecificationAssignments
{
    private readonly Dictionary<Type, Dictionary<int, object?>> _assignments = [];
    private readonly Dictionary<Type, Dictionary<int, string>> _tagNames = [];

    internal void TagIndex(Type type, int index, string tagName)
    {
        var typedTagNames = _tagNames.TryGetValue(type, out var val) ? val : _tagNames[type] = [];
        typedTagNames[index] = tagName;
    }

    internal void Assign(Type type, int index, object? value)
    {
        var typedAssignments = _assignments.TryGetValue(type, out var val) ? val : _assignments[type] = [];
        typedAssignments[index] = value;
    }

    internal string ListAssignments()
    {
        StringBuilder sb = new();
        foreach (var typedAssignments in _assignments)
            foreach (var assignment in typedAssignments.Value)
                sb.AppendLine(DescribeAssignment(typedAssignments.Key, assignment.Key, assignment.Value));
        return sb.ToString();
    }

    private string DescribeAssignment(Type type, int index, object? value) 
        => $"{type.Alias()}:{GetIdentifier(type, index)} = {value.ParseValue()}";

    private string GetIdentifier(Type type, int index)
        => _tagNames.TryGetValue(type, out var typedTagNames)
            && typedTagNames.TryGetValue(index, out var val) ? val
            : $"{index + 1}";
}