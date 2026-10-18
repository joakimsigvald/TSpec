namespace TSpec.Architecture.CSharp;

/// <summary>
/// What a declared member is
/// </summary>
public enum MemberKind
{
    /// <summary>
    /// A method
    /// </summary>
    Method,

    /// <summary>
    /// A function declared inside a method's body
    /// </summary>
    LocalFunction,

    /// <summary>
    /// A constructor, a primary one too
    /// </summary>
    Constructor,

    /// <summary>
    /// A finalizer: ~Name()
    /// </summary>
    Destructor,

    /// <summary>
    /// A property, a record's positional parameter too
    /// </summary>
    Property,

    /// <summary>
    /// An indexer: this[...]
    /// </summary>
    Indexer,

    /// <summary>
    /// A field: each variable of a declaration on its own
    /// </summary>
    Field,

    /// <summary>
    /// An event
    /// </summary>
    Event,

    /// <summary>
    /// An operator
    /// </summary>
    Operator,

    /// <summary>
    /// An implicit or explicit conversion operator
    /// </summary>
    Conversion,

    /// <summary>
    /// A value of an enum
    /// </summary>
    EnumMember,

    /// <summary>
    /// A delegate
    /// </summary>
    Delegate,
}
