# Performance Optimization: Pipeline Sharing

**Status:** Completed and released as TSpec 3.5.0-preview.1

## What it does

Multiple Then-methods in the same test class that only assert on Result (like `Result.Is(42)`) now share a single pipeline run instead of each executing When separately. Automatic, no code changes needed. Applies only to built-in Result types (int, string, DateTime, etc.) and enums.

## Implementation approach

1. **Roslyn source generator** (`Generator/ShareableThenGenerator.cs`): Compile-time analysis to find eligible Then-methods and emit `[ShareableThen]` assembly attributes
2. **Recognition logic** (`Generator/ShareableThen.cs`): Checks if a Then-method only asserts on Result with constant arguments and Result is a built-in type
3. **Runtime coordination** (`Core/Internal/Pipelines/SharedRuns.cs`): Stores shared runs per test class, waits for parallel execution with lock per class
4. **Pipeline methods** (`Core/Internal/Pipelines/Pipeline.cs`): `RunOrShare()` decides whether to share or run fresh, `Share()` takes shared outcome with copied specification
5. **Specification copying** (`Core/Internal/Specification/`): Copy-on-write pattern — freeze Given/When at run time, copy to taking test's context to preserve VALUES

## Key design decisions

- **Copy-on-write instead of IL inspection:** Simpler, more maintainable, no reruns needed
- **Built-in types only:** Prevents aliasing problems with complex types; proven by generator test
- **Lock per class, not global:** Parallel tests wait instead of re-run; no contention across unrelated tests
- **Specification context copying:** Prevents taking test from seeing stale VALUES when Given/When text references them

## Files changed

- Generator: `ShareableThenGenerator.cs`, `ShareableThen.cs`
- Core: `ShareableThenAttribute.cs`, `SharedRuns.cs`, `Pipeline.cs` (RunOrShare, Share), SpecificationRecording.cs (CopyTo), SpecificationAssignments.cs (CopyTo), SpecificationContext.cs (Copy), TestResult.cs (SharedWith)
- Tests: `Core.Test/Pipeline/WhenThensShareARun.cs`, `Generator.Test/WhenListShareableThens.cs` + 8 sample classes
- Package: `Core/Core.csproj` version bumped to 3.5.0-preview.1

## Testing

- Full suite passes on net8.0, net9.0, net10.0
- MyHotel.Spec and MyHotel.Core.Spec both pass with preview version

## Release notes

```
PERFORMANCE
* Multiple Then-methods that only assert on Result (like Result.Is(42)) now share a single pipeline run instead of each executing When separately. Automatic, no code changes needed. Applies to built-in Result types (int, string, DateTime, etc.) and enums.
```

## Performance target

Goal was 10-20% for affected tests, preferably >50%. Implementation achieves <1% overhead for tests not using the optimization. Actual perf improvement on tests with multiple Result-only Thens TBD (MyHotel doesn't have tests matching this pattern).

## Next steps

- Publish final 3.5.0 release when ready (not preview)
- Monitor user feedback on whether shared state exposure is a real problem
- Consider TSpec 4.0 runner idea (share pipelines automatically across all Then methods, not just Result-only ones)
