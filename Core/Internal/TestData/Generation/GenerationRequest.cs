using System.Collections.Immutable;

namespace TSpec.Internal.TestData.Generation;

internal record GenerationRequest(
    Type Type,
    bool WithDefaultFallback,
    ImmutableStack<Type> Stack,
    DataGenerator Orchestrator,
    For Scope)
{
    internal object? Create(Type type) => Orchestrator.Create(this with { Type = type });
    internal GenerationRequest Next => this with { WithDefaultFallback = true, Stack = Stack.Push(Type) };
}