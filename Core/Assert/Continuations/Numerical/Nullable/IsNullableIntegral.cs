using System.Numerics;

namespace TSpec.Assert.Continuations.Numerical.Nullable;

/// <summary>
/// Object that allows assertions to be made on the provided nullable integral number
/// </summary>
/// <typeparam name="TActual">The integral type of the value to assert on</typeparam>
public record IsNullableIntegral<TActual>
    : IsNullableNumerical<TActual, IsNullableIntegral<TActual>, IsIntegral<TActual>>
    where TActual : struct, IBinaryInteger<TActual>;
