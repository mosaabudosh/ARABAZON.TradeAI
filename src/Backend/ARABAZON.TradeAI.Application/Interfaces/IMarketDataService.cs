using ARABAZON.TradeAI.Domain.Entities;
using ARABAZON.TradeAI.Domain.Enums;

namespace ARABAZON.TradeAI.Application.Interfaces;

public interface IMarketDataService
{
    Task<IEnumerable<Candle>> GetCandlesAsync(string symbol, Timeframe timeframe, int count = 100, CancellationToken ct = default);
    Task<MarketSnapshot> GetLiveSnapshotAsync(string symbol, CancellationToken ct = default);
    Task<bool> IsConnectedAsync(CancellationToken ct = default);
}