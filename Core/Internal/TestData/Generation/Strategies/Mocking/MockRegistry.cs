using Moq;
using System.Collections.Concurrent;
namespace TSpec.Internal.TestData.Generation.Strategies.Mocking;

internal class MockRegistry(FluentDefaultProvider defaultProvider)
{
    private readonly ConcurrentDictionary<Type, Mock> _mocks = [];

    internal Mock GetMock(Type type) => _mocks.GetOrAdd(type, CreateMock);

    internal Mock<TObject> GetMock<TObject>() where TObject : class
        => (Mock<TObject>)GetMock(typeof(TObject));

    private Mock CreateMock(Type type)
    {
        var mockType = typeof(Mock<>).MakeGenericType(type);
        var mock = (Mock)Activator.CreateInstance(mockType)!;
        mock.DefaultValueProvider = defaultProvider;
        return mock;
    }
}