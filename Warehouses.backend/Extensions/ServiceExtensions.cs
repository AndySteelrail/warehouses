using Warehouses.backend.Repositories;
using Warehouses.backend.Repositories.Interfaces;
using Warehouses.backend.Services;

namespace Warehouses.backend;

public static class ServiceExtensions
{
    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        return services
            .AddScoped<IWarehouseRepository, WarehouseRepository>()
            .AddScoped<IPicketRepository, PicketRepository>()
            .AddScoped<IPlatformRepository, PlatformRepository>()
            .AddScoped<IPlatformPicketRepository, PlatformPicketRepository>()
            .AddScoped<ICargoRepository, CargoRepository>()
            .AddScoped<ICargoTypeRepository, CargoTypeRepository>();
    }

    public static IServiceCollection AddServices(this IServiceCollection services)
    {
        return services
            .AddScoped<IWarehouseService, WarehouseService>()
            .AddScoped<IPlatformService, PlatformService>()
            .AddScoped<IPicketService, PicketService>()
            .AddScoped<ICargoService, CargoService>()
            .AddScoped<IPlatformValidationService, PlatformValidationService>()
            .AddScoped<IPlatformCreationService, PlatformCreationService>();
    }
}