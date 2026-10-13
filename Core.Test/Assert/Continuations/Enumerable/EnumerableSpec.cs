namespace TSpec.Test.Assert.Continuations.Enumerable;

public abstract class EnumerableSpec : Spec
{
    protected static string Describe<TItem>(IEnumerable<TItem> collection)
        => collection is null ? "null" : $"{collection.Count()}: [{string.Join(", ", collection)}]";
}