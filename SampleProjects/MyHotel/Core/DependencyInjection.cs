using Microsoft.Extensions.DependencyInjection;
using MyHotel.Contract;
using MyHotel.Core.Bookings;
using MyHotel.Core.Rooms;

namespace MyHotel.Core;

public static class DependencyInjection
{
    public static IServiceCollection AddCore(this IServiceCollection services)
        => services
            .AddSingleton<IRoomService, RoomService>()
            .AddSingleton<IBookingService, BookingService>();
}
