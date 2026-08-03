using Microsoft.Extensions.DependencyInjection;
using MyHotel.Core.Bookings;
using MyHotel.Core.Rooms;

namespace MyHotel.Infra;

public static class DependencyInjection
{
    public static IServiceCollection AddInfra(
        this IServiceCollection services, string roomsPath, string bookingsPath)
        => services
            .AddSingleton<IRoomStore>(new RoomStore(roomsPath))
            .AddSingleton<IBookingStore>(new BookingStore(bookingsPath));
}
