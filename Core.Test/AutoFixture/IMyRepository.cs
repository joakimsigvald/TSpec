namespace TSpec.Test.AutoFixture;

public interface IMyRepository
{
    MyModel Get(int id);
    MyModel[] List();
    MyModel[] Create(int count);
}