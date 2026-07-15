namespace TSpec.Internal.TestData.Generation.Strategies;

internal class TypeConversionStrategy() : IGenerationStrategy
{
    private static readonly For[] _scopes = [For.All, For.Input, For.Subject];
    private readonly Dictionary<Type, TypeRelay> _generalRelays = [];
    private readonly Dictionary<Type, TypeRelay> _inputRelays = [];
    private readonly Dictionary<Type, TypeRelay> _subjectRelays = [];

    public bool TryGenerate(GenerationRequest request, ref object? result)
    {
        if (!TryGetRelay(request.Type, request.Scope, out var source))
            return false;

        result = source.Sequence.Next is { } next ? next()
            : source.Type == request.Type ? throw new SetupFailed(
                $"Using<{request.Type.Name}>().From<{request.Type.Name}>() with the same type requires a sequence, e.g. StartingAt or Spaced")
            : request.Create(source.Type);

        if (source.Convert != null)
        {
            result = source.Convert(result);
            return true;
        }

        if (request.Type.IsAssignableFrom(source.Type))
            return true;

        result = GetCandidateValues(result)
            .Select(TryConvert)
            .FirstOrDefault(v => v != null)
            ?? TryChangeType(result)
            ?? throw new InvalidTypeConversion(request.Type, source.Type);

        return true;

        IEnumerable<object?> GetCandidateValues(object? sourceVal)
            => source.Type
                .GetMethods(
                System.Reflection.BindingFlags.Public |
                System.Reflection.BindingFlags.Static |
                System.Reflection.BindingFlags.FlattenHierarchy)
                .Where(m => m.Name == "op_Implicit" && m.GetParameters().Length == 1)
                .Select(m => m.Invoke(null, [sourceVal]))
                .Prepend(sourceVal);

        object? TryConvert(object? argument)
            => TryConstruct(argument)
            ?? TryStatic(argument);

        object? TryStatic(object? argument)
        {
            var argType = argument?.GetType() ?? source.Type;
            var method = request.Type
                .GetMethods(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)
                .FirstOrDefault(m => m.ReturnType == request.Type
                && m.GetParameters() is [{ ParameterType: var pt }]
                && pt.IsAssignableFrom(argType));
            try { return method?.Invoke(null, [argument]); }
            catch { return null; }
        }

        object? TryConstruct(object? argument)
            => request.Type.GetConstructor([argument?.GetType() ?? source.Type])?.Invoke([argument]);

        object? TryChangeType(object? argument)
        {
            try { return Convert.ChangeType(argument, request.Type); }
            catch { return null; }
        }
    }

    internal void Register<TTarget, TSource>(Func<TSource, TTarget>? convert, For scope, SequenceHolder sequence)
    {
        var overlapping = _scopes.FirstOrDefault(
            existing => (existing & scope) != For.None && GetRelays(existing).ContainsKey(typeof(TTarget)));
        if (overlapping != For.None)
            throw new SetupFailed(
                $"Using<{typeof(TTarget).Name}> for {scope} overlaps the existing registration for {overlapping}. Scopes for the same type must be disjoint");
        GetRelays(scope)[typeof(TTarget)] = new(typeof(TSource), convert is null ? null : value => convert((TSource)value!), sequence);
    }

    private bool TryGetRelay(Type type, For scope, out TypeRelay relay)
        => GetRelays(scope).TryGetValue(type, out relay!)
        || _generalRelays.TryGetValue(type, out relay!);

    private Dictionary<Type, TypeRelay> GetRelays(For scope)
        => scope switch
        {
            For.Input => _inputRelays,
            For.Subject => _subjectRelays,
            For.All => _generalRelays,
            _ => throw new NotImplementedException($"{scope}")
        };
}

internal record TypeRelay(Type Type, Func<object?, object?>? Convert, SequenceHolder Sequence);

/// <summary>
/// Holds the optional value sequence of a type relay, installed after registration by StartingAt or Spaced.
/// </summary>
internal class SequenceHolder
{
    internal Func<object?>? Next;
}