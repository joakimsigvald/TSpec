using System.Collections.Generic;
using TSpec.Assert;

namespace TSpec.Test.Characterization;

public class WhenSUTHasIEnumerableArg : Spec<EnumerableHolder, IEnumerable<int>>
{
    public WhenSUTHasIEnumerableArg() => When(_ => _.Get());

    [Fact]
    public void Then_IEnumerableIsInjected_AndNonNull()
    {
        Then().Result.Is().Not(null!);
        Specification.Is(
            """
            When _.Get()
            Then Result is not null
            """);
    }
}

public class WhenSUTHasIListArg : Spec<ListHolder, IList<string>>
{
    public WhenSUTHasIListArg() => When(_ => _.Get());

    [Fact]
    public void Then_IListIsInjected_AndNonNull()
    {
        Then().Result.Is().Not(null!);
        Specification.Is(
            """
            When _.Get()
            Then Result is not null
            """);
    }
}

public class WhenSUTHasIDictionaryArg : Spec<DictionaryHolder, IDictionary<string, int>>
{
    public WhenSUTHasIDictionaryArg() => When(_ => _.Get());

    [Fact]
    public void Then_IDictionaryIsInjected_AndNonNull()
    {
        Then().Result.Is().Not(null!);
        Specification.Is(
            """
            When _.Get()
            Then Result is not null
            """);
    }
}

public class WhenSUTHasIReadOnlyListArg : Spec<ReadOnlyListHolder, IReadOnlyList<int>>
{
    public WhenSUTHasIReadOnlyListArg() => When(_ => _.Get());

    [Fact]
    public void Then_IReadOnlyListIsInjected_AndNonNull()
    {
        Then().Result.Is().Not(null!);
        Specification.Is(
            """
            When _.Get()
            Then Result is not null
            """);
    }
}

public class EnumerableHolder(IEnumerable<int> items)
{
    private readonly IEnumerable<int> _items = items;
    public IEnumerable<int> Get() => _items;
}

public class ListHolder(IList<string> items)
{
    private readonly IList<string> _items = items;
    public IList<string> Get() => _items;
}

public class DictionaryHolder(IDictionary<string, int> map)
{
    private readonly IDictionary<string, int> _map = map;
    public IDictionary<string, int> Get() => _map;
}

public class ReadOnlyListHolder(IReadOnlyList<int> items)
{
    private readonly IReadOnlyList<int> _items = items;
    public IReadOnlyList<int> Get() => _items;
}
