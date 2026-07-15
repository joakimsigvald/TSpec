using Moq;
using System.Linq.Expressions;
using TSpec.Continuations;
using TSpec.Internal.Specification;
using TSpec.Internal.TestData;
using TSpec.Internal.Verification;

namespace TSpec.Internal.Pipelines;

internal class Pipeline<TSUT, TResult> : Fixture<TSUT>
{
    private TestResult<TSUT, TResult>? _result;

    internal ITestResultWithSUT<TSUT, TResult> Then(string? because)
    {
        Specification.SetSubject(null);
        if (because is not null)
            Specification.AddBecause(because);
        return TestResult;
    }

    internal TSubject Then<TSubject>(TSubject subject, string subjectExpr)
    {
        subjectExpr.AssertNoTrainwreck();
        Specification.SetSubject(subjectExpr);
        _ = TestResult;
        return subject;
    }

    internal IAndVerify<TResult> Then<TService>(
        Expression<Action<TService>> expression, string expressionExpr)
        where TService : class
        => TestResult.Verify(expression, expressionExpr);

    internal IAndVerify<TResult> Then<TService>(
        Expression<Action<TService>> expression, Times times, string expressionExpr) where TService : class
        => TestResult.Verify(expression, times, expressionExpr);

    internal IAndVerify<TResult> Then<TService>(
        Expression<Action<TService>> expression, Func<Times> times, string expressionExpr) where TService : class
        => TestResult.Verify(expression, times, expressionExpr);

    internal IAndVerify<TResult> Then<TService, TReturns>(
        Expression<Func<TService, TReturns>> expression, string expressionExpr) where TService : class
        => TestResult.Verify(expression, expressionExpr);

    internal IAndVerify<TResult> Then<TService, TReturns>(
        Expression<Func<TService, TReturns>> expression, Times times, string expressionExpr)
        where TService : class
        => TestResult.Verify(expression, times, expressionExpr);

    internal IAndVerify<TResult> Then<TService, TReturns>(
        Expression<Func<TService, TReturns>> expression, Func<Times> times, string expressionExpr)
        where TService : class
        => TestResult.Verify(expression, times, expressionExpr);

    internal TValue Mention<TValue>(int? index = 0) => _context.Produce<TValue>(index);

    internal TValue Mention<TValue>(Tag<TValue> tag, string tagName) => _context.Produce(tag, tagName);

    internal TValue Assign<TValue>(Tag<TValue> tag, TValue value, string tagName)
    {
        AssertHasNotRun();
        return _context.Assign(tag, value, tagName);
    }

    internal TValue Apply<TValue>(Tag<TValue> tag, Mutation<TValue> mutation, string tagName)
    {
        AssertHasNotRun();
        return _context.Apply(tag, mutation, tagName);
    }

    internal TValue Create<TValue>(Action<TValue> setup) => ApplyTo(setup, _context.Create<TValue>());

    private static TValue ApplyTo<TValue>(Action<TValue> setup, TValue value)
    {
        setup.Invoke(value);
        return value;
    }

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
        => _context.MentionMany<TValue>(count, minCount);

    internal TValue[] AssignMany<TValue>(TValue[] values)
        => _context.AssignMany(values);

    internal TValue[] ApplyMany<TValue>(Mutation<TValue> mutation, int count)
        => _context.ApplyMany(mutation, count);

    internal void SetAction(Delegate act, string actExpr)
    {
        if (_methodUnderTest is not null)
            throw new SetupFailed("Cannot call When twice in the same pipeline");
        _methodUnderTest = new(act ?? throw new SetupFailed("Act cannot be null"), actExpr);
    }

    internal TestResult<TSUT, TResult> TestResult => _result ??= Run();

    private TestResult<TSUT, TResult> Run()
    {
        PrepareToExecute();
        return Execute();
    }

    private void PrepareToExecute()
    {
        if (!_fixture.IsSetUp)
            _fixture.SetUp(Arrange());
        Specification.AddWhen(MethodUnderTest.Expression);
        _fixture.AddToSpecification();
    }

    private TestResult<TSUT, TResult> Execute()
    {
        try
        {
            var (result, hasResult) = _fixture.Invoke<TResult>(MethodUnderTest);
            return new(_fixture.SubjectUnderTest, result, null, _context, hasResult);
        }
        catch (Exception ex) when (ex is not SetupFailed)
        {
            return new(_fixture.SubjectUnderTest, default!, ex, _context, false);
        }
    }

    private Command MethodUnderTest => _methodUnderTest ?? throw new SetupFailed("When must be called before Then or Result");

    private void AssertHasNotRun()
    {
        if (_result != null)
            throw new SetupFailed("Cannot provide setup after test pipeline was run");
    }
}