using TSpec.Assert;
using Xunit.Sdk;

namespace TSpec.Test.Assert.Continuations.Enumerable.HasEnumerable;

public class WhenOrder : Spec
{
    private record Patient(string Name, DateTime Admitted);

    [Theory]
    [InlineData(1, 1)]
    [InlineData(4, 2, 1, -1)]
    public void GivenDescending(params int[] numbers)
    {
        numbers.Has().Order().Descending();
        Specification.Is($"Numbers has descending order");
    }

    [Theory]
    [InlineData("Expected numbers to be descending but found [1, 3]", 1, 3)]
    public void GivenDescendingFail(string errorMessage, params int[] numbers)
    {
        var ex = Xunit.Assert.Throws<XunitException>(() => numbers.Has().Order().Descending());
        ex.Message.Is(errorMessage);
    }

    [Theory]
    [InlineData(1, 2)]
    public void GivenAsceding(params int[] numbers)
    {
        numbers.Has().Order().Ascending();
        Specification.Is($"Numbers has ascending order");
    }

    [Theory]
    [InlineData("Expected numbers to be ascending but found [2, 1]", 2, 1)]
    public void GivenAscedingFail(string errorMessage, params int[] numbers)
    {
        var ex = Xunit.Assert.Throws<XunitException>(() => numbers.Has().Order().Ascending());
        ex.Message.Is(errorMessage);
    }

    [Fact]
    public void GivenExplicitTypeArgument()
    {
        int[] numbers = [1, 2, 3];
        numbers.Has().Order<int>().Ascending();
        numbers.Has().Order<int>(it => it % 4).Ascending();
    }

    [Fact]
    public void GivenDescendingBy()
    {
        int[] numbers = [2, 1, 3];
        numbers.Has().Order(it => it % 3).Descending();
        Specification.Is($"Numbers has descending order by it => it % 3");
    }

    [Fact]
    public void GivenDescendingByFail()
    {
        int[] numbers = [3, 1, 2];
        var ex = Xunit.Assert.Throws<XunitException>(() => numbers.Has().Order(it => it % 3).Descending());
        ex.Message.Is("Expected numbers to be descending by it => it % 3 but found [3, 1, 2]");
    }

    [Fact]
    public void GivenAscendingBy()
    {
        int[] numbers = [3, 1, 2];
        numbers.Has().Order(it => it % 3).Ascending();
        Specification.Is($"Numbers has ascending order by it => it % 3");
    }

    [Fact]
    public void GivenAscendingByFail()
    {
        int[] numbers = [2, 1, 3];
        var ex = Xunit.Assert.Throws<XunitException>(() => numbers.Has().Order(it => it % 3).Ascending());
        ex.Message.Is("Expected numbers to be ascending by it => it % 3 but found [2, 1, 3]");
    }

    [Fact]
    public void GivenStrings()
    {
        string[] names = ["Adam", "Bertil", "Cesar"];
        names.Has().Order().Ascending();
        Specification.Is($"Names has ascending order");
    }

    [Fact]
    public void GivenStringsFail()
    {
        string[] names = ["Bertil", "Adam"];
        var ex = Xunit.Assert.Throws<XunitException>(() => names.Has().Order().Ascending());
        ex.Message.Is("Expected names to be ascending but found [\"Bertil\", \"Adam\"]");
    }

    [Fact]
    public void GivenNonComparableItemsWithStringKey()
    {
        Patient[] patients =
            [new("Adam", new(2026, 3, 1)), new("Bertil", new(2026, 2, 1)), new("Cesar", new(2026, 1, 1))];
        patients.Has().Order(p => p.Name).Ascending();
        Specification.Is($"Patients has ascending order by p => p.Name");
    }

    [Fact]
    public void GivenNonComparableItemsWithDateTimeKey()
    {
        Patient[] patients =
            [new("Adam", new(2026, 3, 1)), new("Bertil", new(2026, 2, 1)), new("Cesar", new(2026, 1, 1))];
        patients.Has().Order(p => p.Admitted).Descending();
        Specification.Is($"Patients has descending order by p => p.Admitted");
    }

    [Fact]
    public void GivenNonComparableItemsWithKeyFail()
    {
        Patient[] patients = [new("Bertil", new(2026, 2, 1)), new("Adam", new(2026, 3, 1))];
        var ex = Xunit.Assert.Throws<XunitException>(() => patients.Has().Order(p => p.Name).Ascending());
        ex.Message.Is("Expected patients to be ascending by p => p.Name but found [Patient { Name = Bertil, Admitted = 2026-02-01 00:..., Patient { Name = Adam, Admitted = 2026-03-01 00:00...]");
    }

    [Fact]
    public void GivenNullKeys()
    {
        Patient?[] patients = [null, new("Adam", new(2026, 3, 1))];
        patients.Has().Order(p => p?.Name!).Ascending();
    }

    [Fact]
    public void GivenChainedContinuation()
    {
        int[] numbers = [1, 2, 3];
        numbers.Has().Order().Ascending().and.Count(3);
    }

    [Fact]
    public void GivenChainedContinuationBy()
    {
        Patient[] patients = [new("Adam", new(2026, 3, 1)), new("Bertil", new(2026, 2, 1))];
        patients.Has().Order(p => p.Name).Ascending().and.Count(2);
    }
}
