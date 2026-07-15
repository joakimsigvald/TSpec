namespace TSpec.Test.AutoFixture;

public class MyRetriever(IMyRepository repository)
{
    private readonly IMyRepository _repository = repository;

    public MyModel Get(int id) => _repository.Get(id);
    public MyModel[] List() => _repository.List();
    public MyModel[] Create(int count) => _repository.Create(count);
}