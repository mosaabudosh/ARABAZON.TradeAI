using ARABAZON.TradeAI.Application.Interfaces;
using ARABAZON.TradeAI.Infrastructure.MarketData;
using ARABAZON.TradeAI.Infrastructure.Persistence;
using ARABAZON.TradeAI.Infrastructure.Services;
using ARABAZON.TradeAI.Infrastructure.Strategies;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ARABAZON.TradeAI.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IDateTimeProvider, DateTimeProvider>();
        services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();
        services.AddScoped<ICandleRepository, CandleRepository>();

        // Strategies
        services.AddScoped<IStrategyEngine, EmaRsiAtrStrategy>();
        services.AddScoped<IStrategyEngineFactory, StrategyEngineFactory>();

        // MT5 Bridge HTTP client
        var mt5BridgeUrl = configuration["MT5Bridge:BaseUrl"] ?? "http://localhost:8001";
        services.AddHttpClient<IMarketDataService, MT5MarketDataAdapter>(client =>
        {
            client.BaseAddress = new Uri(mt5BridgeUrl);
            client.Timeout = TimeSpan.FromSeconds(5);
        });

        return services;
    }
}