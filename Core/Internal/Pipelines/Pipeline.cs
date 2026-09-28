using System.Linq.Expressions;
using TSpec.Continuations;
using TSpec.Internal.Document;
using TSpec.Internal.Mocking;
using TSpec.Internal.Specification;
using TSpec.Internal.TestData;
using TSpec.Internal.Verification;

namespace TSpec.Internal.Pipelines;

internal class Pipeline<TSUT, TResult> : Fixture<TSUT>
{
    private TestResult<TSUT, TResult>? _result;

    /// Opens a statement for the assertion to fill. Reading the result first runs the pipeline,
    /// so the claim always follows the act it is about.
    internal TestResult<TSUT, TResult> Claim
    {
        get
        {
            var result = TestResult;
            Specification.MakeCurrent();
            Specification.AddThen();
            return result;
        }
    }

    internal ITestResultWithSUT<TSUT, TResult> Then(string? because)
    {
        Specification.ClearSubject();
        if (because is not null)
            Specification.AddBecause(because);
        return Claim;
    }

    /// A subject handed over before the run may be one of the Fact's own fields, which only a run of
    /// its own fills, so the Fact runs alone.
    internal TSubject Then<TSubject>(TSubject subject, string subjectExpr)
    {
        subjectExpr.AssertNoTrainwreck();
        HandedOverSubject.AssertIsNotALambda(subject, subjectExpr);
        HandedOverSubject.AssertIsNotACopyTakenBeforeTheRun<TSubject>(_phase.Current == Phase.Assert, subjectExpr);
        Specification.SetSubject(subjectExpr);
        NoteUse();
        _ = Claim;
        return subject;
    }

    internal IAndVerify<TResult> ThenWasInvoked<TService>(
        MockTarget<TService> target, Times wasInvoked, string wasInvokedExpr) where TService : class
        => Claim.VerifyInvoked(target, wasInvoked, wasInvokedExpr);

    internal IAndVerify<TResult> Then<TService>(
        MockTarget<TService> target, string method, Times wasInvoked, string wasInvokedExpr) where TService : class
        => Claim.VerifyInvoked(target, method, wasInvoked, wasInvokedExpr);

    internal IAndVerify<TResult> Then<TService>(
        MockTarget<TService> target, LambdaExpression call, Times? wasInvoked, string callExpr, string? wasInvokedExpr)
        where TService : class
        => Claim.VerifyCall(target, call, wasInvoked, callExpr, wasInvokedExpr);

    internal TValue Mention<TValue>(int? index = 0) => Values().Mention<TValue>(index);

    internal TValue Mention<TValue>(Tag<TValue> tag) => Values().Mention(tag);

    internal TValue Assign<TValue>(Tag<TValue> tag, TValue value)
    {
        AssertHasNotRun();
        return _context.Assign(tag, value);
    }

    internal TValue Apply<TValue>(Tag<TValue> tag, Mutation<TValue> mutation)
    {
        AssertHasNotRun();
        return _context.Apply(tag, mutation);
    }

    internal TValue Create<TValue>(Action<TValue> setup) => Values().Create(setup);

    internal TValue Apply<TValue>(Mutation<TValue> mutation, int? index = null)
    {
        AssertHasNotRun();
        return _context.Apply(mutation, index);
    }

    internal TValue Assign<TValue>(int index, TValue value)
    {
        AssertHasNotRun();
        return _context.Assign(value, index);
    }

    internal TValue[] MentionMany<TValue>(int count, int? minCount = null)
        => Values().MentionMany<TValue>(count, minCount);

    internal TValue[] AssignMany<TValue>(TValue[] values)
        => Values().AssignMany(values);

    internal TValue[] ApplyMany<TValue>(Mutation<TValue> mutation, int count)
        => Values().ApplyMany(mutation, count);

    /// <summary>
    /// Whether the act was given the subject, and whether it yields a result. Each <c>When</c>
    /// overload knows both from its own signature, so they are stated rather than inferred — the
    /// document names a declared type only where the spec uses it in that capacity.
    /// </summary>
    internal bool ActsOnSubject { get; private set; }

    internal bool YieldsResult { get; private set; }

    internal void SetAction(Delegate act, string actExpr, bool actsOnSubject, bool yieldsResult)
    {
        NoteUse();
        if (_methodUnderTest is not null)
            throw new SetupFailed("Cannot call When twice in the same pipeline");
        _methodUnderTest = new(act ?? throw new SetupFailed("Act cannot be null"), actExpr);
        ActsOnSubject = actsOnSubject;
        YieldsResult = yieldsResult;
    }

    internal TestResult<TSUT, TResult> TestResult => _result ??= RunOrShare();

    internal IReadOnlyList<string> RanBefore { get; private set; } = [];

    /// A Fact that used the pipeline before reading the outcome set it up, or holds values of its own.
    private TestResult<TSUT, TResult> RunOrShare()
        => !_usedByTestMethod && SharedRuns.TryGetTest(this, out var test) ? Share(test) : Run();

    /// <summary>
    /// Takes the run of the first Fact in the class to read the outcome first, with what that run
    /// specified and what its test's fields hold, as if it were this Fact's own. The Fact that makes
    /// the run takes it the same way.
    /// </summary>
    private TestResult<TSUT, TResult> Share(object test)
    {
        var run = SharedRuns.GetOrRun(test.GetType(), () => RunToShare(test));
        RanBefore = [.. run.Facts];
        run.Facts.Add(TestIdentity.Requirement);
        TakeOver(run.Maker);
        run.Specification.CopyTo(Specification);
        TestFields.Copy(run.Test, test);
        return run.Outcome;
    }

    private SharedRun<TSUT, TResult> RunToShare(object test) => new(Run(), this, Specification.Copy(), test);

    private Context Values()
    {
        NoteUse();
        return _context;
    }

    /// <summary>
    /// Marks a setup failure on its way out, so that a pipeline enclosing this one can tell it from
    /// one of its own: a failure that has left a pipeline came from a nested specification and is an
    /// outcome of the enclosing act, while the enclosing pipeline's own has not left anything yet.
    /// </summary>
    private TestResult<TSUT, TResult> Run()
    {
        try
        {
            PrepareToExecute();
            var result = Execute();
            _phase.AdvanceTo(Phase.Assert);
            return result;
        }
        catch (SetupFailed ex)
        {
            ex.MarkLeftItsPipeline();
            Specification.NoteSetupFailure();
            throw;
        }
    }

    private void PrepareToExecute()
    {
        _fixture.SetUp(Arrange());
        Specification.AddWhen(MethodUnderTest.Expression);
        _fixture.AddToSpecification();
    }

    /// <summary>
    /// The act runs under a context of its own, which is discarded. An act may use TSpec.Assert
    /// itself, and what it records internally is not the claim — the claim is what the act did,
    /// stated by the Then that follows. Without the scope such an assertion lands in this
    /// specification under no Then, and a failing one freezes it while building its own failure
    /// message, so the real assertion is never recorded at all.
    /// </summary>
    private TestResult<TSUT, TResult> Execute()
    {
        _phase.AdvanceTo(Phase.Act);
        var act = SpecificationContext.Create();
        try
        {
            var (result, hasResult) = _fixture.Invoke<TResult>(MethodUnderTest);
            return new(_fixture.SubjectUnderTest, result, null, _context, hasResult);
        }
        catch (Exception ex) when (ex is not SetupFailed setup || setup.LeftItsPipeline)
        {
            return new(_fixture.SubjectUnderTest, default!, ex, _context, false);
        }
        finally
        {
            act.Release();
        }
    }

    private Command MethodUnderTest => _methodUnderTest ?? throw new SetupFailed("When must be called before Then or Result");

    /// A spec that never provided a When is not driving this pipeline, and a test that failed is not
    /// green for no reason, so both are left alone.
    internal void AssertClaimed()
    {
        if (_methodUnderTest is not null && !TestIdentity.Failed)
            Specification.AssertClaimed();
    }

    private void AssertHasNotRun()
    {
        NoteUse();
        if (_phase.Current > Phase.Act)
            throw new SetupFailed("Cannot provide setup after test pipeline was run");
    }
}