#if !NET9_0_OR_GREATER
namespace System.Runtime.CompilerServices;

/// <summary>
/// Polyfill of the .NET 9 attribute, so the Task overloads can outrank the ValueTask ones on every
/// target framework. The compiler recognizes it by name, from source or metadata.
/// </summary>
[AttributeUsage(
    AttributeTargets.Method | AttributeTargets.Constructor | AttributeTargets.Property,
    AllowMultiple = false, Inherited = false)]
internal sealed class OverloadResolutionPriorityAttribute(int priority) : Attribute
{
    public int Priority => priority;
}
#endif
