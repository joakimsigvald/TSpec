using System.Reflection;
using TSpec.Internal.Document;
using Xunit.v3;

namespace TSpec.Internal.Pipelines;

/// <summary>
/// A Fact that failed on a run it took from earlier Facts is run again alone. If it passes then, one
/// of them likely changed what it reads, and the test output says so — by the time a test is
/// disposed, its failure message is already fixed.
/// </summary>
internal static class CollisionHint
{
    internal static void WriteIfItPassesAlone(Type testClass, IReadOnlyList<string> ranBefore)
    {
        if (ranBefore.Count == 0 || !TestIdentity.Failed)
            return;

        if (TestContext.Current.TestMethod is IXunitTestMethod { Method: var method } && PassesAlone(testClass, method))
            TestContext.Current.TestOutputHelper?.WriteLine(Hint(method.Name, ranBefore));
    }

    /// A class whose constructor takes a fixture cannot be made here, and is taken not to pass.
    internal static bool PassesAlone(Type testClass, MethodInfo method)
    {
        try
        {
            using var instance = (IDisposable)Activator.CreateInstance(testClass, nonPublic: true)!;
            var returned = method.Invoke(instance, null);
            AsyncHelper.Execute(() => Completion(returned));
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    internal static string Hint(string fact, IReadOnlyList<string> ranBefore)
        => $"{fact} passes when run alone. It shared its run with {string.Join(", ", ranBefore)}, which ran "
            + $"before it and may have changed what it reads. Move {fact} to a class of its own.";

    private static Task Completion(object? returned)
        => returned switch
        {
            Task task => task,
            ValueTask valueTask => valueTask.AsTask(),
            _ => Task.CompletedTask,
        };
}
