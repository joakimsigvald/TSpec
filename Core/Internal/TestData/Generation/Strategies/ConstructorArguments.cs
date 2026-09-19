using TSpec.Internal.TestData.Generation.Strategies.IlCompilation;

namespace TSpec.Internal.TestData.Generation.Strategies;

/// <summary>
/// The arguments a generated value's constructor is called with. A parameter with a default value says
/// what the class runs without, so building the subject fills it with what the test arranged and lets
/// the default stand for the rest.
/// </summary>
internal static class ConstructorArguments
{
    internal static object?[] Of(CompiledParameter[] parameters, GenerationRequest request, List<string> honouredDefaults)
        => [.. parameters.Select(parameter => Fill(parameter, request, honouredDefaults))];

    private static object? Fill(CompiledParameter parameter, GenerationRequest request, List<string> honouredDefaults)
    {
        if (!parameter.HasDefault || !request.Scope.HasFlag(For.Subject))
            return request.Next.Create(parameter.Type);
        if (request.Next.TryCreateFromSetup(parameter.Type, out var arranged))
            return arranged;

        honouredDefaults.Add(parameter.Name);
        return parameter.Default;
    }
}
