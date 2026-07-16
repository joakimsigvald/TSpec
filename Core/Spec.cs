using TSpec.Continuations;
using TSpec.Internal.Pipelines;

namespace TSpec;

/// <summary>
/// Base-class for specifying and executing a set of test cases for a specific method-under-test, with no subject-under-test and no return type
/// </summary>
public abstract class Spec : Spec<object, object>
{
}

/// <summary>
/// Base-class for specifying and executing a set of test cases for a specific method-under-test.
/// This base class should typically be used for static methods, but can also be used to specify subject-under-test but no return type
/// (or when subject-under-test and the return value has the same type)
/// </summary>
/// <typeparam name="TSUTorResult">The type used both as the subject-under-test and as the return type of the method-under-test</typeparam>
public abstract class Spec<TSUTorResult> : Spec<TSUTorResult, TSUTorResult>;

/// <summary>
/// Base-class for specifying and executing a set of test cases for a specific method-under-test
/// </summary>
/// <typeparam name="TSUT">The class to instantiate and execute the method-under-test on</typeparam>
/// <typeparam name="TResult">The return type of the method-under-test</typeparam>
public abstract partial class Spec<TSUT, TResult> : ITestPipeline<TSUT, TResult>, IDisposable
{
    private readonly Lazy<string> _lazySpecification = null!;

    internal Pipeline<TSUT, TResult> Pipeline { get; } = new();

    /// <summary>
    /// Initialize the test pipeline and specification generation
    /// </summary>
    protected Spec() => _lazySpecification = new(Pipeline.Specification.ToString);

    /// <summary>
    /// This property returns the specification of the test, after the test has been run.
    /// The specification will also be included in the message of the exception if the test fails.
    /// Assertions on the specification are line-ending-agnostic.
    /// </summary>
    public SpecificationText Specification => new(_lazySpecification);

    /// <summary>
    /// Runs the teardown of the test pipeline (the steps provided with Until).
    /// After all Until-steps have run, any disposable objects that TSpec created for the
    /// subject-under-test graph (the subject itself and concrete dependencies it was constructed with)
    /// are disposed, in reverse order of creation. Instances provided with Using, mocks and
    /// generated input data are not disposed. To manage the subject's lifetime yourself,
    /// provide your own instance with Using.
    /// </summary>
    public void Dispose()
    {
        Pipeline.TearDown();
        GC.SuppressFinalize(this);
    }
}