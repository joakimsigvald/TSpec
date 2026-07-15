namespace TSpec;

/// <summary>
/// For defines which part of the test pipeline provided values and setups are applied to.
/// They can either be used when constructing the subject under test and mocks (`Subject`),
/// or be applied as defaults for test-data generation (`Input`),
/// or neither (`None`) or both (`All`)
/// </summary>
[Flags]
public enum For
{
    /// <summary>
    /// The value applies neither to Input nor Subject.
    /// </summary>
    None,

    /// <summary>
    /// Used as fallback test data when generating arbitrary values.
    /// </summary>
    Input,

    /// <summary>
    /// Injected directly into the constructor of the Subject Under Test.
    /// </summary>
    Subject,

    /// <summary>
    /// Used both for SUT construction and as default test data.
    /// </summary>
    All
}