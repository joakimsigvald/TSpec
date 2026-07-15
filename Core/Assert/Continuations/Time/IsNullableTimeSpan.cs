namespace TSpec.Assert.Continuations.Time;

/// <summary>
/// Object that allows assertions to be made on the provided nullable TimeSpan
/// </summary>
public record IsNullableTimeSpan : IsNullableComparableStruct<TimeSpan, IsNullableTimeSpan, IsTimeSpan>;