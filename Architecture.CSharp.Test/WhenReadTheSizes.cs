using TSpec.Assert;
using TSpec.Test.Internal.Document;
using static TSpec.Architecture.CSharp.Test.WrittenFiles;

namespace TSpec.Architecture.CSharp.Test;

public class WhenReadTheSizes : Spec
{
    [Fact]
    public void ThenEachLineOfAMemberCostsTwoToItsLevelLessHalfTheLevelTheMemberStartsAt()
        => SizeOf("Run", """
            namespace N
            {
                class A
                {
                    void Run()
                    {
                        Go();
                    }
                }
            }
            """).Is(2 + 2 + 4 + 2);

    [Fact]
    public void GivenALineIndentedBetweenTwoLevels_ThenItsWidthIsALevelOfItsOwn()
        => SizeOf("Go", """
            namespace N
            {
                class A
                {
                    int Run() => 1
                      + 2;
                    void Go()
                    {
                        Stop();
                    }
                }
            }
            """).Is(2 + 2 + 8 + 2);

    [Fact]
    public void GivenABlankLine_ThenItCostsAsALineAtLevelZero()
        => SizeOf("Run", """
            namespace N
            {
                class A
                {
                    void Run()
                    {
                        Go();

                        Stop();
                    }
                }
            }
            """).Is(2 + 2 + 4 + 0.5 + 4 + 2);

    [Fact]
    public void GivenATab_ThenItIndentsToTheNextMultipleOfFour()
        => SizeOf("Run", "namespace N\n{\n    class A\n    {\n        void Run()\n        {\n"
            + "\t\t\tGo();\n  \t\t\tStop();\n        }\n    }\n}\n").Is(2 + 2 + 4 + 4 + 2);

    [Fact]
    public void GivenAnAttributeOnALineOfItsOwn_ThenItIsALineOfTheMember()
        => SizeOf("Run", """
            namespace N
            {
                class A
                {
                    [Fact]
                    void Run() { }
                }
            }
            """).Is(2 + 2);

    [Fact]
    public void GivenAField_ThenItHasASizeToo()
        => SizeOf("_count", """
            namespace N
            {
                class A
                {
                    int _count;
                }
            }
            """).Is(2);

    [Fact]
    public void GivenAPrimaryConstructor_ThenItIsMeasuredOverItsParameterList()
        => SizeOf("A", """
            namespace N
            {
                class Outer
                {
                    class A(int count)
                    {
                        int Count => count;
                    }
                }
            }
            """).Is(2);

    [Fact]
    public void GivenTopLevelStatements_ThenTheFileMeasuresThemAsOneMember()
        => TopLevelSizeOf("""
            var count = 1;
            if (count > 0)
            {
                Go();
            }
            static void Go() { }
            class A { }
            """).Is(1 + 1 + 1 + 2 + 1 + 1);

    [Fact]
    public void GivenNoTopLevelStatements_ThenTheFileHasNoneToMeasure()
        => TopLevelSizeOf("class A { }").Is(0);

    private static double SizeOf(string member, string code)
    {
        using var project = new TempProject("Entry");
        return Written(project, code).Members.Single(declared => declared.Name == member).Size;
    }

    private static double TopLevelSizeOf(string code)
    {
        using var project = new TempProject("Entry");
        return Written(project, code).TopLevelSize;
    }
}
