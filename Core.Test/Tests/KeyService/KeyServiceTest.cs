using TSpec.Assert;
using TSpec.Test.Subjects.RecordStructDefaults;

namespace TSpec.Test.Tests.KeyService;

public class KeyServiceSpec : Spec<Subjects.RecordStructDefaults.KeyService, Key<string, string>>
{
}

public class WhenKeyKey : KeyServiceSpec
{
    public WhenKeyKey() => When(_ => _.GetKey());

    [Fact]
    public void ThenGetsKey()
    {
        Result.A.Is().not.NullOrEmpty();
        Specification.Is(
            """
            When _.GetKey()
            Then Result.A is not null or empty
            """);
    }
}