namespace TSpec.Internal.Specification;

/// <summary>
/// Describes the arrangement steps: Given/Using values and mock behavior.
/// What a step says is decided here; whether it reads "Given ..." or "and ...",
/// and whether a mocked service is named again, is decided while rendering.
/// </summary>
internal class SetupPhrases(SpecificationRecording recording)
{
    internal void AddGiven(string valueExpr, For scope)
        => RecordSetup(() => Given(scope switch
        {
            For.Subject => $"using {valueExpr.Describe()}",
            For.Input => $"{valueExpr.Describe()} is default",
            _ => valueExpr.Describe(),
        }));

    internal void AddGiven<TValue>(string setupExpr, bool isCustomExpression, string? article)
        => RecordSetup(() => Given(isCustomExpression
            ? setupExpr
            : $"{ArticlePrefix(article)}{DescribeSetupExpression<TValue>(setupExpr, article)}"));

    internal void AddGivenCount<TModel>(string count)
        => RecordSetup(() => Given($"{ArticlePrefix(count)}{typeof(TModel).Alias().CountedBy(count)}"));

    internal void AddGivenThat(string customArrangementExpr)
        => recording.Record(() => Given($"that {customArrangementExpr.Describe()}"));

    internal void AddUsing(string valueExpr, For scope, bool owned = false)
        => RecordSetup(() => Using(valueExpr, scope, owned));

    internal void AddUsing(Func<bool> shouldRender, string valueExpr, For scope)
        => RecordSetup(() =>
        {
            if (shouldRender())
                Using(valueExpr, scope, owned: false);
        });

    internal void AddUsingConversion<TTarget, TSource>(For scope, Func<string> describeSequence)
        => RecordSetup(() => Add(StepLayout.SentenceOrPhrase, StepFamily.Using,
            $"{typeof(TTarget).Alias()} from {typeof(TSource).Alias()}{describeSequence()}{ScopeSuffix(scope)}"));

    internal void AddUsingFactory<TTarget>(For scope, string generateExpr)
        => RecordSetup(() => Add(StepLayout.SentenceOrPhrase, StepFamily.Using,
            $"{typeof(TTarget).Alias()} from {generateExpr}{ScopeSuffix(scope)}"));

    internal void AddMockSetup<TService>(string callExpr)
        => recording.Record(() => Mock<TService>(
            StepLayout.SentenceOrPhrase, callExpr.DescribeCall(true) ?? string.Empty, '.'));

    internal void AddMockReturnsDefault<TService>(string returnsExpr)
        => recording.Record(() => Mock<TService>(
            StepLayout.SentenceOrPhrase, $"returns {returnsExpr.Describe()}"));

    internal void AddMockReturns(string? returnsExpr)
        => recording.Record(() => Add(
            StepLayout.Word, StepFamily.None, $"returns {returnsExpr?.Describe()}".Trim()));

    internal void AddMockThrowsDefault<TService, TException>()
        => recording.Record(() => Mock<TService>(
            StepLayout.Word, $"throws {typeof(TException).Alias()}"));

    internal void AddMockThrowsDefault<TService>(string expectedExpr)
        => recording.Record(() => Mock<TService>(
            StepLayout.Word, $"throws {expectedExpr.Describe()}"));

    internal void AddMockThrows<TException>()
        => recording.Record(() => Add(
            StepLayout.Word, StepFamily.None, $"throws {typeof(TException).Alias()}"));

    internal void AddMockThrows(string expectedExpr)
        => recording.Record(() => Add(
            StepLayout.Word, StepFamily.None, $"throws {expectedExpr.Describe()}"));

    /// A described setup step ends any mock setup in progress, so a later mock
    /// phrase names its service again. The run ends even when the step itself
    /// turns out to describe nothing.
    private void RecordSetup(Action describe)
        => recording.Record(() =>
        {
            recording.Add(new(StepLayout.Silent) { EndsMockRun = true });
            describe();
        });

    private void Given(string body) => Add(StepLayout.SentenceOrPhrase, StepFamily.Given, body);

    private void Using(string valueExpr, For scope, bool owned)
        => Add(StepLayout.SentenceOrPhrase, StepFamily.Using,
            $"{(owned ? "owned " : "")}{valueExpr.Describe()}{ScopeSuffix(scope)}");

    private void Mock<TService>(StepLayout layout, string body, char binder = ' ')
        => recording.Add(new(layout)
        {
            Family = StepFamily.Given,
            Body = body,
            MockService = typeof(TService).Alias(),
            MockBinder = binder,
        });

    private void Add(StepLayout layout, StepFamily family, string body)
        => recording.Add(new(layout) { Family = family, Body = body });

    /// <summary>
    /// An articled value is a thing the author asked for, so it reads as the noun phrase it is —
    /// "a MyModel with Name = …". Without an article the same setup states a rule about every value
    /// of the type, which needs a verb to stay a sentence: "MyModel has Name = …". The article also
    /// carries the count, so "with" is what keeps a plural agreeing.
    /// </summary>
    private static string DescribeSetupExpression<TValue>(string setupExpr, string? article)
    {
        var value = setupExpr.Describe();
        var verb = !value.Contains('=') || value.StartsWith("new") ? "is"
            : string.IsNullOrEmpty(article) ? "has"
            : "with";
        return $"{typeof(TValue).Alias().CountedBy(article ?? string.Empty)} {verb} {value}";
    }

    private static string ArticlePrefix(string? article)
        => string.IsNullOrEmpty(article) ? string.Empty : $"{article.AsWords()} ";

    private static string ScopeSuffix(For scope) => scope == For.All ? string.Empty : $" for {scope}";
}
