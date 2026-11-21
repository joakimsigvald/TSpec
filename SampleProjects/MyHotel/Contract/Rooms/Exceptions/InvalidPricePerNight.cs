namespace MyHotel.Contract.Rooms.Exceptions;

public class InvalidPricePerNight(int pricePerNight)
    : Exception($"A room's price per night cannot be {pricePerNight} kr.")
{
    public int PricePerNight { get; } = pricePerNight;
}
