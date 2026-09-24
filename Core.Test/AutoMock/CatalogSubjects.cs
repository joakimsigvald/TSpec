namespace TSpec.Test.AutoMock;

/// A service whose members, named, stand for every kind a name can reach: overloads, async members,
/// a property, a generic method, an out parameter, and members answering with nothing.
public interface ICatalog
{
    int Count(string shelf);
    string Find(int id);
    string Find(string code);
    object Find(Guid key);
    int Find(bool archived);
    Task<string> FindAsync(int id);
    ValueTask<string> FindSoonAsync(int id);
    string Title { get; set; }
    T Read<T>(string key);
    bool TryFind(int id, out string entry);
    void Note(string entry);
    Task SaveAsync(string entry);
    ValueTask FlushAsync();
}

public abstract class Shelf
{
    public virtual string Label(int row) => "real";
    public string Label(string name) => "real";
    public string Fixed() => "real";
    public string StampOf(int row) => Stamp(row);
    protected abstract string Stamp(int row);
}

public class CatalogService(ICatalog catalog, Shelf shelf)
{
    public string CountOf(string shelf) => catalog.Count(shelf).ToString();
    public string CountTwice(string first, string second) => $"{catalog.Count(first)},{catalog.Count(second)}";
    public string FindById(int id) => catalog.Find(id);
    public string FindByCode(string code) => catalog.Find(code);
    public string FindByKey(Guid key) => $"{catalog.Find(key)}";
    public string CountArchived() => catalog.Find(true).ToString();
    public async Task<string> FindByIdAsync(int id) => await catalog.FindAsync(id);
    public async Task<string> FindSoon(int id) => await catalog.FindSoonAsync(id);
    public string ReadTitle() => catalog.Title;

    public string Retitle(string title)
    {
        catalog.Title = title;
        return catalog.Title;
    }

    public string ReadText(string key) => catalog.Read<string>(key);
    public string ReadNumber(string key) => catalog.Read<int>(key).ToString();
    public string TryFind(int id) => $"{catalog.TryFind(id, out var entry)}:{entry}";
    public void Note(string entry) => catalog.Note(entry);
    public Task Save(string entry) => catalog.SaveAsync(entry);
    public ValueTask Flush() => catalog.FlushAsync();
    public string LabelOfRow(int row) => shelf.Label(row);
    public string LabelOfName(string name) => shelf.Label(name);
    public string FixedLabel() => shelf.Fixed();
    public string StampOfRow(int row) => shelf.StampOf(row);
}
