using System.Reflection;

namespace TSpec.Internal.Mocking;

/// <summary>
/// Every call to a member of a name that can answer with what the setup answers, whatever its
/// arguments: a method, or a property read, whose return type can hold the answer, or whose task can.
/// A setup answering with nothing reaches the members that answer with nothing.
/// </summary>
internal sealed class NameMatcher(string name, Type answerType) : ICallMatcher
{
    public bool Matches(MethodInfo method, IReadOnlyList<object?> arguments)
        => NameOf(method) == name && CanAnswer(method.ReturnType, answerType);

    public void WriteOutArguments(object?[] arguments) { }

    /// A property is named by its read.
    internal static string NameOf(MethodInfo method)
        => PropertyAccess.IsRead(method) ? method.Name["get_".Length..] : method.Name;

    /// A generic member's return type is its type parameter until it is called, when the type argument decides.
    internal static bool CanAnswer(Type returnType, Type answerType)
        => answerType == typeof(Continuations.Void) ? AnswersNothing(returnType) : CanHold(returnType, answerType);

    private static bool AnswersNothing(Type returnType)
        => returnType == typeof(void) || returnType == typeof(Task) || returnType == typeof(ValueTask);

    private static bool CanHold(Type returnType, Type answerType)
        => returnType.ContainsGenericParameters
        || returnType.IsAssignableFrom(answerType)
        || AsyncAnswer.IsAsyncOfValue(returnType) && returnType.GenericTypeArguments[0].IsAssignableFrom(answerType);
}
