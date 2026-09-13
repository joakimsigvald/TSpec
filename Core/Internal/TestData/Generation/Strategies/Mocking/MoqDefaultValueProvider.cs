using Moq;

namespace TSpec.Internal.TestData.Generation.Strategies.Mocking;

/// <summary>
/// Moq asks for the default of an unarranged call through a provider of its own; the answer is TSpec's,
/// given for the mock the provider was made for.
/// </summary>
internal sealed class MoqDefaultValueProvider(MockHandle mock, FluentDefaultProvider defaults) : DefaultValueProvider
{
    protected override object GetDefaultValue(Type type, Mock _) => defaults.GetDefaultValue(type, mock);
}
