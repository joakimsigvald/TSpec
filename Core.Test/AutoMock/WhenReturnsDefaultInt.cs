using TSpec.Assert;
using TSpec.Test.AutoFixture;

namespace TSpec.Test.AutoMock;

public class WhenReturnsDefaultInt : Spec<MyDefaultService, int>
{
    [Fact]
    public void ThenReturnZero()
    {
        Given<IDefaultRetriever>().That(_ => _.GetInt()).ReturnsDefault()
            .When(_ => _.GetInt()).Then().Result.Is(0);
        Specification.Is(
            """
            Given IDefaultRetriever.GetInt() returns default
            When _.GetInt()
            Then Result is 0
            """);
    }
}

public class WhenReturnsDefaultModel : Spec<MyDefaultService, MyModel>
{
    [Fact]
    public void ThenReturnNull()
    {
        Given<IDefaultRetriever>().That(_ => _.GetModel()).ReturnsDefault()
            .When(_ => _.GetModel()).Then().Result.Is().Null();
        Specification.Is(
            """
            Given IDefaultRetriever.GetModel() returns default
            When _.GetModel()
            Then Result is null
            """);
    }
}

public interface IDefaultRetriever
{
    int GetInt();
    object GetObject();
    MyModel GetModel();
}

public class MyDefaultService(IDefaultRetriever retriever)
{
    private readonly IDefaultRetriever _retriever = retriever;
    public int GetInt() => _retriever.GetInt();
    public object GetObject() => _retriever.GetObject();
    public MyModel GetModel() => _retriever.GetModel();
}