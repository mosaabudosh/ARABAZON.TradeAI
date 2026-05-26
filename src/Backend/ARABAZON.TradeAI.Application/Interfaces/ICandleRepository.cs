using ARABAZON.TradeAI.Domain.Entities;
using ARABAZON.TradeAI.Domain.Enums;

namespace ARABAZON.TradeAI.Application.Interfaces;

public interface ICandleRepository
{
    Task SaveCandlesAsync(IEnumerable<Candle> candles, CancellationToken ct = default);
    Task<IEnumerable<Candle>> GetCandlesAsync(Guid symbolId, Timeframe timeframe, DateTime from, DateTime to, CancellationToken ct = default);
    Task<Candle?> GetLatestCandleAsync(Guid symbolId, Timeframe timeframe, CancellationToken ct = default);
}