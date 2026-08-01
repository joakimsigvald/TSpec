using Microsoft.Extensions.DependencyInjection;
using MyHotel.Contract;

namespace MyHotel.Core;

public static class DependencyInjection
{
    public static IServiceCollection AddCore(this IServiceCollection services)
        => services.AddSingleton<IRoomService, RoomService>();
}
