namespace TSpec.Generator.Test.Samples;

public abstract class WhenGetNumbers : Spec<object, List<int>>
{
    [Fact] public void ThenItHasOne() => Result.Has().Count(1);
}
