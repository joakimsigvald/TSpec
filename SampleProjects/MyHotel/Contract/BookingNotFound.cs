namespace MyHotel.Contract;

public class BookingNotFound(int id)
    : Exception($"There is no booking {id}.")
{
    public int Id { get; } = id;
}
