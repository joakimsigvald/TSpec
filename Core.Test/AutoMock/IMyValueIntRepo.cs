namespace TSpec.Test.AutoMock;

public interface IMyValueIntRepo
{
    void Set(int value);
    string Get(int value);
    string Get2(int value1, int value2);
    Task SetAsync(int value);
    Task<string> GetAsync(int value);
    IMyValueIntRepo GetMe();
    Task<IMyValueIntRepo> GetMeAsync();
    object GetObject();
    Task<object> GetObjectAsync();
}