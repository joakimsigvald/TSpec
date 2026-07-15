namespace TSpec.Test.Subjects;

public class Calculator
{
    public static int Add(int x, int y) => x + y;
    public static decimal? Add(decimal? x, decimal? y) => x + y;
    public static float? Add(float? x, float? y) => x + y;
    public static int Multiply(int x, int y) => x * y;

    public static Task<int> AddAsync(int x, int y) => Task.FromResult(x + y);
}