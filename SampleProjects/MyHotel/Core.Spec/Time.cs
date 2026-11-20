namespace MyHotel.Core.Spec;

public static class Time
{
    public static readonly DateOnly Today = new(2026, 8, 10);
    public static readonly DateOnly Tomorrow = Today.AddDays(1);
    public static readonly DateOnly Yesterday = Today.AddDays(-1);
    public static readonly DateOnly LaterDay = Today.AddDays(9);
    public static readonly DateOnly EarlierDay = Today.AddDays(-9);
}
