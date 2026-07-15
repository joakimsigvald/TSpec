namespace TSpec.Test.Assert.Continuations.String;

public abstract class StringSpec : Spec
{
    protected static string Describe(string? value) => value is null ? "null" : $"\"{value}\"";
}