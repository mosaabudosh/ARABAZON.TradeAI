using ARABAZON.TradeAI.Application.Interfaces;
using ARABAZON.TradeAI.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;

namespace ARABAZON.TradeAI.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddSingleton<IDateTimeProvider, DateTimeProvider>();
        services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();
        return services;
    }
}