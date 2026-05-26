namespace ARABAZON.TradeAI.Application.Interfaces;

public interface IStrategyEngineFactory
{
    IEnumerable<IStrategyEngine> GetActiveStrategies();
}