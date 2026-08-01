using Microsoft.Extensions.DependencyInjection;

namespace MyHotel.Entry;

public static class DependencyInjection
{
    public static IServiceCollection AddEntry(this IServiceCollection services)
        => services.AddExceptionHandler<GlobalExceptionHandler>();
}
