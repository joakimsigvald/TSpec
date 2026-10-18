namespace TSpec.Architecture.CSharp.Test.Exposes;

public class Exposed;

public class Returning
{
    public Exposed Get() => new();
}

public class Deriving : Exposed;

public class SelfNamed
{
    public SelfNamed Self() => this;
}

public class Hiding
{
    public object Use() => Get();

    private Exposed Get() => new();
}

internal class Unseen
{
    public Exposed Get() => new();
}
