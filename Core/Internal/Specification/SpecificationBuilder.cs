namespace TSpec.Internal.Specification;

internal class SpecificationBuilder
{
    private readonly List<Action> _applications = new(10);
    private int _givenCount;
    private int _usingCount;
    private int _recordingSuppressionCount;
    private int _thenCount;
    private string? _currentMockSetup;
    private readonly TextBuilder _textBuilder = new();
    private bool _isChainOfAssertions = false;

    private string? _cachedSpecification;
    private string? _because;

    public override string ToString()
    {
        if (_cachedSpecification is not null)
            return _cachedSpecification;

        foreach (var apply in _applications)
            apply();

        if (_because is not null)
            _textBuilder.AddWord($"because {_because}", ", ");

        return _cachedSpecification = _textBuilder.ToString();
    }

    internal void SetBecause(string reason)
    {
        if (_recordingSuppressionCount > 0)
            return;
        if (_because is not null)
            throw new SetupFailed("Because can only be provided once per test method");
        _because = reason;
    }

    internal void Add(Action apply)
    {
        if (_recordingSuppressionCount > 0)
            return;
        _applications.Add(apply);
    }

    internal void AddMockSetup<TService>(string callExpr)
        => _textBuilder.AddPhraseOrSentence($"{Given} {GetMockName<TService>('.')}{callExpr.ParseCall(true)}");

    internal void AddMockReturnsDefault<TService>(string returnsExpr)
        => _textBuilder.AddPhraseOrSentence($"{Given} {GetMockName<TService>(' ')}returns {returnsExpr.ParseValue()}");

    internal void AddMockReturns(string? returnsExpr)
        => _textBuilder.AddWord($"returns {returnsExpr?.ParseValue()}".Trim());

    internal void AddMockThrowsDefault<TService, TException>()
        => _textBuilder.AddWord($"{Given} {GetMockName<TService>(' ')}throws {NameOf<TException>()}");

    internal void AddMockThrowsDefault<TService>(string expectedExpr)
        => _textBuilder.AddWord($"{Given} {GetMockName<TService>(' ')}throws {expectedExpr.ParseValue()}");

    internal void AddMockThrows<TException>()
        => _textBuilder.AddWord($"throws {NameOf<TException>()}");

    internal void AddMockThrows(string expectedExpr)
        => _textBuilder.AddWord($"throws {expectedExpr.ParseValue()}");

    internal void AddWhen(string actExpr)
        => _textBuilder.AddSentence($"when {actExpr.ParseCall()}");

    internal void AddAfter(string setUpExpr)
        => _textBuilder.AddSentence($"after {setUpExpr.ParseCall()}");

    internal void AddBefore(string tearDownExpr)
        => _textBuilder.AddSentence($"before {tearDownExpr.ParseCall()}");

    internal void AddAssert(string actual, string verb, string? expected)
    {
        // actual is already described text, not source code — never re-parse it
        if (_isChainOfAssertions)
            _textBuilder.AddWord(actual);
        else
            _textBuilder.AddSentence(actual);
        _textBuilder.AddWord(verb.AsWords());
        _textBuilder.AddWord(expected.ParseValue());
        _isChainOfAssertions = false;
    }

    internal void AddThen()
    {
        _isChainOfAssertions = true;
        _textBuilder.AddPhraseOrSentence(Then);
    }

    internal void AddThat()
    {
        _isChainOfAssertions = true;
        _textBuilder.AddWord(_that);
    }

    internal void AddGiven(string valueExpr, For scope)
    {
        _currentMockSetup = null;
        _textBuilder.AddPhraseOrSentence(string.Join(' ', GetWords()));

        IEnumerable<string> GetWords()
        {
            yield return Given;
            if (scope == For.Subject)
                yield return "using";
            yield return valueExpr.ParseValue();
            if (scope == For.Input)
                yield return "is default";
        }
    }

    internal void AddUsing(string valueExpr, For scope)
    {
        _currentMockSetup = null;
        _textBuilder.AddPhraseOrSentence(string.Join(' ', GetWords()));

        IEnumerable<string> GetWords()
        {
            yield return Using;
            yield return valueExpr.ParseValue();
            if (scope != For.All)
                yield return $"for {scope}";
        }
    }

    internal void AddUsingConversion<TTarget, TSource>(For scope, Func<string> describeSequence)
    {
        _currentMockSetup = null;
        _textBuilder.AddPhraseOrSentence($"{Using} {NameOf<TTarget>()} from {NameOf<TSource>()}{describeSequence()}{ScopeSuffix(scope)}");
    }

    internal void AddUsingFactory<TTarget>(For scope, string generateExpr)
    {
        _currentMockSetup = null;
        _textBuilder.AddPhraseOrSentence($"{Using} {NameOf<TTarget>()} from {generateExpr}{ScopeSuffix(scope)}");
    }

    private static string ScopeSuffix(For scope) => scope == For.All ? string.Empty : $" for {scope}";

    internal void AddGiven<TValue>(string setupExpr, bool isCustomExpression, string? article)
    {
        _currentMockSetup = null;
        _textBuilder.AddPhraseOrSentence(GetGivenExpression<TValue>(setupExpr, isCustomExpression, article));
    }

    private string GetGivenExpression<TValue>(string setupExpr, bool isCustomExpression, string? article)
    {
        if (isCustomExpression)
            return $"{Given} {setupExpr}";

        var articleStr = string.IsNullOrEmpty(article) ? string.Empty : $"{article.AsWords()} ";
        return $"{Given} {articleStr}{ParseSetupExpression<TValue>(setupExpr)}";
    }

    private static string ParseSetupExpression<TValue>(string setupExpr)
    {
        var value = setupExpr.ParseValue();
        var verb = value.Contains('=') && !value.StartsWith("new") ? "has" : "is";
        return $"{NameOf<TValue>()} {verb} {value}";
    }

    internal void AddGivenCount<TModel>(string count)
    {
        _currentMockSetup = null;
        var articleStr = string.IsNullOrEmpty(count) ? string.Empty : $"{count.AsWords()} ";
        _textBuilder.AddPhraseOrSentence($"{Given} {articleStr}{NameOf<TModel>()}");
    }

    internal void AddGivenThat(string customArrangementExpr)
        => _textBuilder.AddPhraseOrSentence($"{Given} that {customArrangementExpr.ParseValue()}");

    internal void AddVerify<TService>(string expressionExpr)
        => _textBuilder.AddWord($"{NameOf<TService>()}.{expressionExpr.ParseCall(true)}");

    internal void AddAssertThrows<TError>(string? binder)
        => _textBuilder.AddWord($"throws {NameOf<TError>()} {binder}".Trim());

    internal void AddAssertThrows(string expectedExpr)
        => _textBuilder.AddWord($"throws {expectedExpr.ParseValue()}");

    internal void AddTap(string expr) => _textBuilder.AddWord($"tap({expr})");

    internal void AddAssert(string assertName) => _textBuilder.AddWord(assertName.AsWords());

    internal void AddAssertConjunction(string conjunction)
    {
        _isChainOfAssertions = true;
        _textBuilder.AddPhrase(conjunction, 2);
    }

    private string GetMockName<TService>(char binder)
    {
        var nextMockSetup = NameOf<TService>();
        var mockName = nextMockSetup == _currentMockSetup
            ? ""
            : $"{nextMockSetup}{binder}";
        _currentMockSetup = nextMockSetup;
        return mockName;
    }

    private static string NameOf<T>() => typeof(T).Alias();

    internal void SuppressRecording() => _recordingSuppressionCount++;

    internal void InciteRecording() => _recordingSuppressionCount--;

    private string Given => 0 == _givenCount++ ? "Given" : "and";

    private string Using => 0 == _usingCount++ ? "Using" : "and";

    private string Then => 0 == _thenCount++ ? "Then" : "and";

    private const string _that = "that";
}