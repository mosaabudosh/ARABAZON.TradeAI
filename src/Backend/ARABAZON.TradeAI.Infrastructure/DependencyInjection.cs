using ARABAZON.TradeAI.Application.Interfaces;
using ARABAZON.TradeAI.Infrastructure.Brokers;
using ARABAZON.TradeAI.Infrastructure.MarketData;
using ARABAZON.TradeAI.Infrastructure.Persistence;
using ARABAZON.TradeAI.Infrastructure.Risk;
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
        // Singletons
        services.AddSingleton<IDateTimeProvider, DateTimeProvider>();
        services.AddSingleton<IDailyRiskTracker, DailyRiskTracker>();

        // Scoped
        services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();
        services.AddScoped<ICandleRepository, CandleRepository>();
        services.AddScoped<IPositionSizingService, PositionSizingService>();
        services.AddScoped<IRiskEngine, RiskEngine>();
        services.AddScoped<ITradeExecutionService, TradeExecutionService>();

        // Strategies
        services.AddScoped<IStrategyEngine, EmaRsiAtrStrategy>();
        services.AddScoped<IStrategyEngineFactory, StrategyEngineFactory>();

        var mt5BridgeUrl = configuration["MT5Bridge:BaseUrl"] ?? "http://localhost:8001";

        // MT5 Market Data
        services.AddHttpClient<IMarketDataService, MT5MarketDataAdapter>(client =>
        {
            client.BaseAddress = new Uri(mt5BridgeUrl);
            client.Timeout = TimeSpan.FromSeconds(5);
        });

        // MT5 Execution
        services.AddHttpClient<IMT5ExecutionAdapter, MT5ExecutionAdapter>(client =>
        {
            client.BaseAddress = new Uri(mt5BridgeUrl);
            client.Timeout = TimeSpan.FromSeconds(8);
        });

        return services;
    }
}