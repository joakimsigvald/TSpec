namespace TSpec.Test.TestData;

public interface IMyRepository
{
    MyModel GetModel();
    MyModel SetModel(MyModel model);
    MyModel[] GetModels();
    Task<IEnumerable<MyModel>> GetModelsAsync();
    int GetNextId();
    int[] GetIds();
}