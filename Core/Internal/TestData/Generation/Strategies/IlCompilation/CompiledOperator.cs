namespace TSpec.Internal.TestData.Generation.Strategies.IlCompilation;

internal readonly record struct CompiledOperator(
    Func<object, object>? Instantiate,
    Type? ParameterType
);