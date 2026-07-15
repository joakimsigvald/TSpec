namespace TSpec.Test.TestData;

public class MyService(IMyRepository repo, IMySettings settings, Func<DateTime> getTime)
{
    public string GetConnectionString() => settings.ConnectionString;
    public MyModel GetModel() => repo.GetModel();
    public MyModel[] GetModels() => repo.GetModels();
    public async Task<MyModel[]> GetModelsAsync()
    {
        var models = await repo.GetModelsAsync();
        return [.. models];
    }

    public int GetNextId() => repo.GetNextId();
    public DateTime GetTime() => getTime();
    public int[] GetIds() => repo.GetIds();
    public static TValue Echo<TValue>(TValue model) => model;
}