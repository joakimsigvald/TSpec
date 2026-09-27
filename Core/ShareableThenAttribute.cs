using System.ComponentModel;

namespace TSpec;

/// <summary>
/// Written by TSpec's source generator for a test method that only asserts on the result.
/// Do not apply it by hand.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
public sealed class ShareableThenAttribute(Type specClass, string method) : Attribute
{
    /// <summary>The class declaring the test method</summary>
    public Type SpecClass { get; } = specClass;

    /// <summary>The name of the test method</summary>
    public string Method { get; } = method;
}
