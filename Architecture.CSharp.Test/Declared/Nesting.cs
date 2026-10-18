namespace TSpec.Architecture.CSharp.Test.Declared.Nesting;

public class Outer
{
    public delegate void Handler();

    public enum Size { Small }

    public class Public
    {
        public record Deeper;
    }

    internal interface IInternal;

    private class Hidden;
}

internal class InternalOuter
{
    public class PublicByName;
}

public interface IOuter
{
    class PublicByDefault;
}

public partial class Split
{
    public partial class Part;
}
