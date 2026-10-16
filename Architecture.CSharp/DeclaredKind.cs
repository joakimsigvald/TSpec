namespace TSpec.Architecture.CSharp;

/// <summary>
/// What a declared type is
/// </summary>
public enum DeclaredKind
{
    /// <summary>
    /// A class that is not a record
    /// </summary>
    Class,

    /// <summary>
    /// A record class
    /// </summary>
    Record,

    /// <summary>
    /// A struct that is not a record
    /// </summary>
    Struct,

    /// <summary>
    /// A record struct
    /// </summary>
    RecordStruct,

    /// <summary>
    /// An interface
    /// </summary>
    Interface,

    /// <summary>
    /// An enum
    /// </summary>
    Enum,

    /// <summary>
    /// A delegate
    /// </summary>
    Delegate,
}
