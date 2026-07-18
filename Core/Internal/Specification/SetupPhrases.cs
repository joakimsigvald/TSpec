namespace TSpec.Internal.Specification;

/// <summary>
/// Phrases for the arrangement steps: Given/Using values and mock behavior.
/// Consecutive phrases share their leading word ("Given ..., and ...") and
/// consecutive phrases for the same mock omit the repeated service name.
/// </summary>
internal class SetupPhrases(SpecificationRecording recording, TextBuilder textBuilder)
{
    private int _givenCount;
    private int _usingCount;
    private string? _currentMockSetup;

    internal void AddGiven(string valueExpr, For scope)
        => RecordSetup(() => textBuilder.AddPhraseOrSentence(scope switch
        {
            For.Subject => $"{NextGivenWord()} using {valueExpr.ParseValue()}",
            For.Input => $"{NextGivenWord()} {valueExpr.ParseValue()} is default",
            _ => $"{NextGivenWord()} {valueExpr.ParseValue()}",
        }));

    internal void AddGiven<TValue>(string setupExpr, bool isCustomExpression, string? article)
        => RecordSetup(() => textBuilder.AddPhraseOrSentence(
            GetGivenExpression<TValue>(setupExpr, isCustomExpression, article)));

    internal void AddGivenCount<TModel>(string count)
        => RecordSetup(() => textBuilder.AddPhraseOrSentence(
            $"{NextGivenWord()} {ArticlePrefix(count)}{typeof(TModel).Alias()}"));

    internal void AddGivenThat(string customArrangementExpr)
        => recording.Record(() => textBuilder.AddPhraseOrSentence(
            $"{NextGivenWord()} that {customArrangementExpr.ParseValue()}"));

    internal void AddUsing(string valueExpr, For scope, bool owned = false)
        => RecordSetup(() => RenderUsing(valueExpr, scope, owned));

    internal void AddUsing(Func<bool> shouldRender, string valueExpr, For scope)
        => RecordSetup(() =>
        {
            if (shouldRender())
                RenderUsing(valueExpr, scope, owned: false);
        });

    internal void AddUsingConversion<TTarget, TSource>(For scope, Func<string> describeSequence)
        => RecordSetup(() => textBuilder.AddPhraseOrSentence(
            $"{NextUsingWord()} {typeof(TTarget).Alias()} from {typeof(TSource).Alias()}{describeSequence()}{ScopeSuffix(scope)}"));

    internal void AddUsingFactory<TTarget>(For scope, string generateExpr)
        => RecordSetup(() => textBuilder.AddPhraseOrSentence(
            $"{NextUsingWord()} {typeof(TTarget).Alias()} from {generateExpr}{ScopeSuffix(scope)}"));

    internal void AddMockSetup<TService>(string callExpr)
        => recording.Record(() => textBuilder.AddPhraseOrSentence(
            $"{NextGivenWord()} {GetMockName<TService>('.')}{callExpr.ParseCall(true)}"));

    internal void AddMockReturnsDefault<TService>(string returnsExpr)
        => recording.Record(() => textBuilder.AddPhraseOrSentence(
            $"{NextGivenWord()} {GetMockName<TService>(' ')}returns {returnsExpr.ParseValue()}"));

    internal void AddMockReturns(string? returnsExpr)
        => recording.Record(() => textBuilder.AddWord($"returns {returnsExpr?.ParseValue()}".Trim()));

    internal void AddMockThrowsDefault<TService, TException>()
        => recording.Record(() => textBuilder.AddWord(
            $"{NextGivenWord()} {GetMockName<TService>(' ')}throws {typeof(TException).Alias()}"));

    internal void AddMockThrowsDefault<TService>(string expectedExpr)
        => recording.Record(() => textBuilder.AddWord(
            $"{NextGivenWord()} {GetMockName<TService>(' ')}throws {expectedExpr.ParseValue()}"));

    internal void AddMockThrows<TException>()
        => recording.Record(() => textBuilder.AddWord($"throws {typeof(TException).Alias()}"));

    internal void AddMockThrows(string expectedExpr)
        => recording.Record(() => textBuilder.AddWord($"throws {expectedExpr.ParseValue()}"));

    /// The rendered step ends any mock setup in progress, so a later mock
    /// phrase names its service again.
    private void RecordSetup(Action render)
        => recording.Record(() =>
        {
            _currentMockSetup = null;
            render();
        });

    private void RenderUsing(string valueExpr, For scope, bool owned)
        => textBuilder.AddPhraseOrSentence(
            $"{NextUsingWord()}{(owned ? " owned" : "")} {valueExpr.ParseValue()}{ScopeSuffix(scope)}");

    private string GetGivenExpression<TValue>(string setupExpr, bool isCustomExpression, string? article)
        => isCustomExpression
            ? $"{NextGivenWord()} {setupExpr}"
            : $"{NextGivenWord()} {ArticlePrefix(article)}{ParseSetupExpression<TValue>(setupExpr)}";

    private static string ParseSetupExpression<TValue>(string setupExpr)
    {
        var value = setupExpr.ParseValue();
        var verb = value.Contains('=') && !value.StartsWith("new") ? "has" : "is";
        return $"{typeof(TValue).Alias()} {verb} {value}";
    }

    private static string ArticlePrefix(string? article)
        => string.IsNullOrEmpty(article) ? string.Empty : $"{article.AsWords()} ";

    private static string ScopeSuffix(For scope) => scope == For.All ? string.Empty : $" for {scope}";

    private string GetMockName<TService>(char binder)
    {
        var nextMockSetup = typeof(TService).Alias();
        var mockName = nextMockSetup == _currentMockSetup
            ? ""
            : $"{nextMockSetup}{binder}";
        _currentMockSetup = nextMockSetup;
        return mockName;
    }

    private string NextGivenWord() => 0 == _givenCount++ ? "Given" : "and";

    private string NextUsingWord() => 0 == _usingCount++ ? "Using" : "and";
}
