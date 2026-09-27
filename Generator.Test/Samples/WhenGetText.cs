namespace TSpec.Generator.Test.Samples;

public abstract class WhenGetText : Spec<object, string>
{
    [Fact] public void ThenItHasA4() => Result.Does().Contain("4");
}
