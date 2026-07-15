using TSpec.Assert;

namespace TSpec.Test.Assert;

public class WhenString : Spec<string>
{
    [Fact]
    public void IsNot()
    {
        "A".Is().Not("B");
        Specification.Is(
            """
            "A" is not "B"
            """);
    }

    [Fact]
    public void IsEmpty()
    {
        "".Is().Empty();
        Specification.Is(
            """
            "" is empty
            """);
    }

    [Fact]
    public void IsNotEmpty()
    {
        "x".Is().not.Empty();
        Specification.Is(
            """
            "x" is not empty
            """);
    }

    [Fact]
    public void DoesContainSubstring()
    {
        "abcd".Does().Contain("abc");
        Specification.Is(
            """
            "abcd" contains "abc"
            """);
    }

    [Fact]
    public void DoesContainAndContainSubstrings()
    {
        "abcd".Does().Contain("abc").and.Contain("bcd");
        Specification.Is(
            """
            "abcd" contains "abc"
                and contains "bcd"
            """);
    }

    [Fact]
    public void DoesContainAndIsNotNull()
    {
        "abcd".Does().Contain("abc").and.Is().not.Null();
        Specification.Is(
            """
            "abcd" contains "abc"
                and is not null
            """);
    }

    [Fact]
    public void DoesNotContainOtherString()
    {
        "abcd".Does().not.Contain("xxx");
        Specification.Is(
            """
            "abcd" does not contain "xxx"
            """);
    }

    [Fact]
    public void IsNotNullAndContainSubstring()
    {
        "abcd".Is().not.Null().and.Does().Contain("abc");
        Specification.Is(
            """
            "abcd" is not null
                and contains "abc"
            """);
    }

    [Fact]
    public void IsNotNullAndIs_Abc()
    {
        "abc".Is().not.Null().but.Is("abc");
        Specification.Is(
            """
            "abc" is not null
                but is "abc"
            """);
    }

    [Fact]
    public void IsNotNullAndStartWithSubstring()
    {
        "abcd".Is().not.Null().and.Does().StartWith("abc");
        Specification.Is(
            """
            "abcd" is not null
                and starts with "abc"
            """);
    }

    [Fact]
    public void IsNotNullAndEndWithSubstring()
    {
        "abcd".Is().not.Null().and.Does().EndWith("bcd");
        Specification.Is(
            """
            "abcd" is not null
                and ends with "bcd"
            """);
    }

    [Fact]
    public void IsNotNullAndNotContainOtherString()
    {
        "abcd".Is().not.Null().and.Does().not.Contain("xxx");
        Specification.Is(
            """
            "abcd" is not null
                and does not contain "xxx"
            """);
    }

    [Fact]
    public void IsNotNullAndNot()
    {
        "A".Is().not.Null().and.Not("B");
        Specification.Is(
            """
            "A" is not null
                and not "B"
            """);
    }

    [Fact]
    public void IsEqual()
    {
        "AbC".Is("AbC");
        Specification.Is(
            """
            "AbC" is "AbC"
            """);
    }

    [Fact]
    public void IsLike()
    {
        "AbC".Is().Like("aBc").and.Like("AbC");
        Specification.Is(
            """
            "AbC" is like "aBc"
                and like "AbC"
            """);
    }

    [Fact]
    public void IsEquivalentTo()
    {
        "AbC".Is().EquivalentTo("aBc").and.EquivalentTo("AbC");
        Specification.Is(
            """
            "AbC" is equivalent to "aBc"
                and equivalent to "AbC"
            """);
    }

    [Fact]
    public void IsNotLike()
    {
        "AbC".Is().not.Like("bac");
        Specification.Is(
            """
            "AbC" is not like "bac"
            """);
    }

    [Fact]
    public void IsNotEquivalentTo()
    {
        "AbC".Is().not.EquivalentTo("bac");
        Specification.Is(
            """
            "AbC" is not equivalent to "bac"
            """);
    }

    [Fact]
    public void DoesStartWith()
    {
        "abcd".Does().StartWith("ab");
        Specification.Is(
            """
            "abcd" starts with "ab"
            """);
    }

    [Fact]
    public void DoesEndWith()
    {
        "abcd".Does().EndWith("cd");
        Specification.Is(
            """
            "abcd" ends with "cd"
            """);
    }

    [Fact]
    public void DoesContainOtherStringFails()
        => Xunit.Assert.Throws<Xunit.Sdk.XunitException>(() => "abcd".Does().Contain("xxx"));

    [Fact]
    public void DoesNotContainSubstringFails()
        => Xunit.Assert.Throws<Xunit.Sdk.XunitException>(() => "abcd".Does().Contain("xxx"));

    [Fact]
    public void WhenOneOf()
    {
        "x".Is().OneOf(["x", "y", "z"]);
        Specification.Is("""
            "x" is one of ["x", "y", "z"]
            """);
    }
}