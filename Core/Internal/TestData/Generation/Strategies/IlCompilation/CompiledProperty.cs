namespace TSpec.Internal.TestData.Generation.Strategies.IlCompilation;

internal readonly record struct CompiledProperty(
    Type PropertyType,
    Func<object, object?> Get,
    Action<object, object?> Set
);