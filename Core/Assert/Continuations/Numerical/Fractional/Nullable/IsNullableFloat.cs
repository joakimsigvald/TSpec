using TSpec.Assert.Continuations.Numerical.Nullable;

namespace TSpec.Assert.Continuations.Numerical.Fractional.Nullable;

/// <summary>
/// Object that allows assertions to be made on the provided nullable float
/// </summary>
public record IsNullableFloat : IsNullableNumerical<float, IsNullableFloat, IsFloat>;