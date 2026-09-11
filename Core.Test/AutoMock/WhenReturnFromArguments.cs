using TSpec.Assert;

namespace TSpec.Test.AutoMock;

/// <summary>
/// An answer computed from several arguments names them, since its body reads them by name; the
/// call already states their types. Written typed, which is how such a lambda is usually spelled.
/// </summary>
public class WhenReturnFromArguments : Spec<Joiner, string>
{
    [Fact]
    public void GivenTwoArguments_ThenNameThem()
    {
        Given<IJoin>().That(_ => _.Join(Any<int>(), Any<int>()))
            .Returns((int a, int b) => $"{a}{b}")
            .When(_ => _.Join2()).Then().Result.Is("12");
        Specification.Is(
            """
            Given IJoin.Join(any int, any int) returns (a, b) => "{a}{b}"
            When Join2()
            Then Result is "12"
            """);
    }

    [Fact]
    public void GivenThreeArguments_ThenNameThemDiscardsIncluded()
    {
        Given<IJoin>().That(_ => _.Join(Any<int>(), Any<int>(), Any<int>()))
            .Returns((int a, int _, int _) => $"{a}")
            .When(_ => _.Join3()).Then().Result.Is("1");
        Specification.Is(
            """
            Given IJoin.Join(any int, any int, any int) returns (a, _, _) => "{a}"
            When Join3()
            Then Result is "1"
            """);
    }

    [Fact]
    public void GivenFourArguments_ThenNameThem()
    {
        Given<IJoin>().That(_ => _.Join(Any<int>(), Any<int>(), Any<int>(), Any<int>()))
            .Returns((int a, int b, int c, int d) => $"{d}")
            .When(_ => _.Join4()).Then().Result.Is("4");
        Specification.Is(
            """
            Given IJoin.Join(any int, any int, any int, any int)
                  returns (a, b, c, d) => "{d}"
            When Join4()
            Then Result is "4"
            """);
    }

    [Fact]
    public void GivenFiveArguments_ThenNameThem()
    {
        Given<IJoin>().That(_ => _.Join(Any<int>(), Any<int>(), Any<int>(), Any<int>(), Any<int>()))
            .Returns((int a, int b, int c, int d, int e) => $"{e}")
            .When(_ => _.Join5()).Then().Result.Is("5");
        Specification.Is(
            """
            Given IJoin.Join(any int, any int, any int, any int, any int)
                  returns (a, b, c, d, e) => "{e}"
            When Join5()
            Then Result is "5"
            """);
    }
}

public interface IJoin
{
    string Join(int a, int b);
    string Join(int a, int b, int c);
    string Join(int a, int b, int c, int d);
    string Join(int a, int b, int c, int d, int e);
}

public class Joiner(IJoin join)
{
    public string Join2() => join.Join(1, 2);
    public string Join3() => join.Join(1, 2, 3);
    public string Join4() => join.Join(1, 2, 3, 4);
    public string Join5() => join.Join(1, 2, 3, 4, 5);
}
