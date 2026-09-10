namespace TSpec.Internal.TestData.Generation.Strategies.IlCompilation;

internal readonly record struct CompiledConstructor(
    Func<object[], object>? Instantiate,
    CompiledParameter[] Parameters
);