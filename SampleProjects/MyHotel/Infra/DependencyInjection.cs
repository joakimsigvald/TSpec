using Microsoft.Extensions.DependencyInjection;
using MyHotel.Core;

namespace MyHotel.Infra;

public static class DependencyInjection
{
    public static IServiceCollection AddInfra(this IServiceCollection services, string roomsPath)
        => services.AddSingleton<IRoomStore>(new RoomStore(roomsPath));
}
