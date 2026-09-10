namespace TSpec.Internal.TestData.Generation.Strategies.IlCompilation;

internal readonly record struct CompiledParameter(
    string Name,
    Type Type,
    bool HasDefault,
    object? Default
);