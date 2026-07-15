namespace TSpec.Test.TestData;

public class MyListService(List<int> _list)
{
    public List<int> List => _list;
}