namespace TSpec.Test.Subjects.RecordStructDefaults;

public class KeyService(IKeyProvider _keyProvider)
{
    public Key<string, string> GetKey() => _keyProvider.GetKey();
}