using TSpec.Test.TestData;

namespace TSpec.Test.AutoMock;

public class MyValueIntService(IMyValueIntRepo repo)
{
    private readonly IMyValueIntRepo _repo = repo;

    public void SetValue(MyValueInt value) => _repo.Set(value);
    public string GetValue(MyValueInt value) => _repo.Get(value);
    public string GetValue2(MyValueInt value1, MyValueInt value2) => _repo.Get2(value1, value2);

    public Task SetValueAsync(MyValueInt value) => _repo.SetAsync(value);
    public Task<string> GetValueAsync(MyValueInt value) => _repo.GetAsync(value);
    public IMyValueIntRepo GetRepo() => _repo.GetMe();
    public Task<IMyValueIntRepo> GetRepoAsync() => _repo.GetMeAsync();
    public object GetObject() => _repo.GetObject();
    public Task<object> GetObjectAsync() => _repo.GetObjectAsync();
    public async Task<string> SetAndGetValueAsync(int value)
    {
        await _repo.SetAsync(value);
        return await _repo.GetAsync(value);
    }
}