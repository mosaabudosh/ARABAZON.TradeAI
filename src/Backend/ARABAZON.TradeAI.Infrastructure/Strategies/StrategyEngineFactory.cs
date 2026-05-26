using ARABAZON.TradeAI.Application.Interfaces;

namespace ARABAZON.TradeAI.Infrastructure.Strategies;

public class StrategyEngineFactory : IStrategyEngineFactory
{
    private readonly IEnumerable<IStrategyEngine> _strategies;

    public StrategyEngineFactory(IEnumerable<IStrategyEngine> strategies)
    {
        _strategies = strategies;
    }

    public IEnumerable<IStrategyEngine> GetActiveStrategies() => _strategies;
}