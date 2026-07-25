using System.Numerics;

namespace TSpec.Assert.Continuations.Numerical;

/// <summary>
/// Object that allows assertions to be made on the provided integral number
/// </summary>
/// <typeparam name="TActual">The integral type of the value to assert on</typeparam>
public record IsIntegral<TActual> : IsNumerical<TActual, IsIntegral<TActual>>
    where TActual : struct, IBinaryInteger<TActual>;
