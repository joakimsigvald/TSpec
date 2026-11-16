namespace TSpec.Architecture.CSharp.Test;

[Serializable]
public class Attributed
{
    [Obsolete]
    public void Marked() { }

    [ObsoleteAttribute]
    public void MarkedWithSuffix() { }

    [System.Obsolete]
    public void MarkedQualified() { }

    public void Plain() { }
}

[Own.Serializable]
public class OwnAttributed;

public partial class AttributedPart;

[Serializable]
public partial class AttributedPart;
