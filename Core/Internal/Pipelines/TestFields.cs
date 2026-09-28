using System.Collections.Concurrent;
using System.Reflection;

namespace TSpec.Internal.Pipelines;

/// <summary>
/// The fields a test class declares, from itself up to TSpec's own base class, copied from the test
/// that made a shared run to one that takes it: what the run's setup and act stored there is what
/// the taking test reads. An output is left alone, since each test writes to its own.
/// </summary>
internal static class TestFields
{
    private const BindingFlags Declared = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
        | BindingFlags.DeclaredOnly;

    private static readonly ConcurrentDictionary<Type, FieldInfo[]> _byClass = new();

    internal static void Copy(object from, object to)
    {
        foreach (var field in _byClass.GetOrAdd(to.GetType(), Copied))
            field.SetValue(to, field.GetValue(from));
    }

    private static FieldInfo[] Copied(Type testClass)
        => [.. AboveTSpec(testClass)
            .SelectMany(type => type.GetFields(Declared))
            .Where(field => !typeof(ITestOutputHelper).IsAssignableFrom(field.FieldType))];

    private static IEnumerable<Type> AboveTSpec(Type testClass)
    {
        for (var type = testClass; type.Assembly != typeof(Spec).Assembly; type = type.BaseType!)
            yield return type;
    }
}
