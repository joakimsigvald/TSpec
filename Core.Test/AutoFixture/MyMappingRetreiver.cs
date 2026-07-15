namespace TSpec.Test.AutoFixture;

public class MyMappingRetreiver(IMyRepository repository, IMyMapper mapper)
{
    public MyModel Get(int id) => mapper.Map(repository.Get(id));
    public MyModel[] List() => repository.List().Select(mapper.Map).ToArray();
}