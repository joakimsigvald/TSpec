namespace TSpec.Internal.TestData.Generation.Strategies;

internal class PrimitiveStrategy(Counter counter) : IGenerationStrategy
{
    internal static readonly DateTime Epoch = new(1999, 9, 18, 12, 0, 0, DateTimeKind.Utc);
    private const int _hundredYears = 36523;
    private const int _oneDay = 60 * 24;

    public bool TryGenerate(GenerationRequest request, ref object? result)
    {
        result = GeneratePrimitive(request.Type);
        return result is not null || TryGenerateBCLStructs(ref result);


        bool TryGenerateBCLStructs(ref object? result)
        {
            if (request.Type == typeof(TimeSpan)) { result = TimeSpan.FromDays(counter.Next); return true; }
            if (request.Type == typeof(DateOnly)) { result = DateOnly.FromDateTime(GetDateTime()); return true; }
            if (request.Type == typeof(TimeOnly)) { result = TimeOnly.FromDateTime(GetDateTime()); return true; }
            if (request.Type == typeof(DateTimeOffset)) { result = (DateTimeOffset)GetDateTime(); return true; }
            if (request.Type == typeof(Uri)) { result = new Uri($"https://xspec.dev/{counter.Next}"); return true; }
            return false;
        }
    }

    private object? GeneratePrimitive(Type type)
        => Type.GetTypeCode(type) switch
        {
            TypeCode.Boolean => counter.Next % 2 == 1,
            TypeCode.DateTime => GetDateTime(),
            TypeCode.Char => (char)('!' + (counter.Next % 94)),
            TypeCode.Int32 => counter.Next,
            TypeCode.UInt32 => (uint)counter.Next,
            TypeCode.Byte => (byte)counter.Next,
            TypeCode.SByte => (sbyte)counter.Next,
            TypeCode.Int16 => (short)counter.Next,
            TypeCode.UInt16 => (ushort)counter.Next,
            TypeCode.Int64 => (long)counter.Next,
            TypeCode.UInt64 => (ulong)counter.Next,
            TypeCode.Single => (float)GetFractional(),
            TypeCode.Double => (double)GetFractional(),
            TypeCode.Decimal => GetFractional(),
            TypeCode.String => $"String{counter.Next}",
            _ => null
        };

    private decimal GetFractional() => counter.Next + (1 + Math.Abs(counter.Current) % 99) / 100m;

    private DateTime GetDateTime() => Epoch.AddDays(counter.Next * 367 % _hundredYears).AddMinutes(counter.Current * 17 % _oneDay);
}