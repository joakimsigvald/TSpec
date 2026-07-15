using TSpec.Assert.Continuations.Numerical.Nullable;

namespace TSpec.Assert.Continuations.Numerical.Fractional.Nullable;

/// <summary>
/// Object that allows assertions to be made on the provided nullable double
/// </summary>
public record IsNullableDouble : IsNullableNumerical<double, IsNullableDouble, IsDouble>;