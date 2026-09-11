using System.Reflection;
using TSpec.Assert;
using TSpec.Internal.Document;

namespace TSpec.Test.Internal.Document;

/// <summary>
/// What the project says of itself: the Description of its project file, which the build compiles
/// into the assembly. Read from the assembly the spec project references, so the one sentence a
/// project already writes for its package heads its specification too.
/// </summary>
public class WhenReadSubjectDescription : Spec
{
    [Fact]
    public void ThenReadTheDescriptionCompiledIntoTheAssembly()
        => SubjectDescription.Of("TSpec")
            .Is(typeof(Spec).Assembly.GetCustomAttribute<AssemblyDescriptionAttribute>()!.Description);

    /// A project with no Description compiles no attribute, and there is nothing to say.
    [Fact]
    public void GivenTheProjectStatesNoDescription_ThenNone()
        => SubjectDescription.Of("TSpec.Test").Is().Null();

    [Fact]
    public void GivenNoSuchAssembly_ThenNone()
        => SubjectDescription.Of("TSpec.NoSuchAssembly").Is().Null();

    /// How a project file lays out its Description is not part of what it says.
    [Fact]
    public void GivenADescriptionOnSeveralIndentedLines_ThenReflowItAsOne()
        => SubjectDescription.Reflowed("\n\t\tThe domain rules,\n\t\tbehind ports.\n\t").Is("The domain rules, behind ports.");
}
