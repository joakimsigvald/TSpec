using TSpec.Assert;
using Xunit.Sdk;

namespace TSpec.Test.Assert;

// Failure descriptions show at most five elements with ellipsis,
// and each element is rendered with its ToString, capped at 50 characters
public class WhenFormatLargeCollections
{
    private record Person(string Name, int Age);

    [Fact]
    public void GivenManyElements_ThenShowFirstFiveWithEllipsis()
    {
        var big = Enumerable.Range(1, 10_000).ToArray();
        var ex = Xunit.Assert.Throws<XunitException>(() => big.Has().Count(3));
        ex.Message.Is("Expected big to have count 3 but found 10000: [1, 2, 3, 4, 5, ...]");
    }

    [Fact]
    public void GivenElementWithLongToString_ThenCapAtFiftyCharacters()
    {
        string[] arr = [new string('a', 60)];
        var ex = Xunit.Assert.Throws<XunitException>(() => arr.Has().Count(2));
        ex.Message.Is($"Expected arr to have count 2 but found 1: [\"{new string('a', 50)}...\"]");
    }

    [Fact]
    public void GivenRecordElements_ThenShowToString()
    {
        Person[] people = [new("Ada", 36)];
        var ex = Xunit.Assert.Throws<XunitException>(() => people.Has().Count(2));
        ex.Message.Is("Expected people to have count 2 but found 1: [Person { Name = Ada, Age = 36 }]");
    }

    [Fact]
    public void GivenTupleElementWithLongToString_ThenCapWithEllipsis()
    {
        (string Name, int Age)[] people = [(new string('a', 60), 36)];
        var ex = Xunit.Assert.Throws<XunitException>(() => people.Has().Count(2));
        ex.Message.Is($"Expected people to have count 2 but found 1: [({new string('a', 49)}...]");
    }
}
