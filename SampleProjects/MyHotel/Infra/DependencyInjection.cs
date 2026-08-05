using Microsoft.Extensions.DependencyInjection;
using MyHotel.Core.Bookings;
using MyHotel.Core.Rooms;

namespace MyHotel.Infra;

public static class DependencyInjection
{
    public static IServiceCollection AddInfra(
        this IServiceCollection services, string roomsPath, string bookingsPath)
    {
        // One store for both: the bookings and the number to issue next share a file.
        var bookings = new BookingStore(bookingsPath);
        return services
            .AddSingleton<IRoomStore>(new RoomStore(roomsPath))
            .AddSingleton<IBookingStore>(bookings)
            .AddSingleton<IBookingNumberStore>(bookings);
    }
}
