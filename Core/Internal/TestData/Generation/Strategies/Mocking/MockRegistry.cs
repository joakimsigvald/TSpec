using Moq;
using System.Collections.Concurrent;
namespace TSpec.Internal.TestData.Generation.Strategies.Mocking;

internal class MockRegistry(FluentDefaultProvider defaultProvider)
{
    private readonly ConcurrentDictionary<Type, MockHandle> _mocks = [];

    internal MockHandle GetMock(Type type) => _mocks.GetOrAdd(type, CreateMock);

    internal bool HasMock(Type type) => _mocks.ContainsKey(type);

    private MockHandle CreateMock(Type type)
    {
        var moqMock = (Mock)Activator.CreateInstance(typeof(Mock<>).MakeGenericType(type))!;
        var mock = new MockHandle(type, moqMock);
        moqMock.DefaultValueProvider = new MoqDefaultValueProvider(mock, defaultProvider);
        return mock;
    }
}
