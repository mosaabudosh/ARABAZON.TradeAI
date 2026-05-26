using ARABAZON.TradeAI.Application.Interfaces;
using ARABAZON.TradeAI.Domain.Entities;
using ARABAZON.TradeAI.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ARABAZON.TradeAI.Infrastructure.Persistence;

public class CandleRepository : ICandleRepository
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<CandleRepository> _logger;

    public CandleRepository(IApplicationDbContext context, ILogger<CandleRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task SaveCandlesAsync(IEnumerable<Candle> candles, CancellationToken ct = default)
    {
        var candleList = candles.ToList();
        if (!candleList.Any()) return;

        // Avoid duplicates — upsert by SymbolId + Timeframe + OpenTime
        foreach (var candle in candleList)
        {
            var exists = await _context.Candles.AnyAsync(
                c => c.SymbolId == candle.SymbolId &&
                     c.Timeframe == candle.Timeframe &&
                     c.OpenTime == candle.OpenTime, ct);

            if (!exists)
                await _context.Candles.AddAsync(candle, ct);
        }

        await _context.SaveChangesAsync(ct);
        _logger.LogDebug("Saved {Count} candles", candleList.Count);
    }

    public async Task<IEnumerable<Candle>> GetCandlesAsync(
        Guid symbolId, Timeframe timeframe, DateTime from, DateTime to, CancellationToken ct = default)
    {
        return await _context.Candles
            .Where(c => c.SymbolId == symbolId &&
                        c.Timeframe == timeframe &&
                        c.OpenTime >= from &&
                        c.OpenTime <= to)
            .OrderBy(c => c.OpenTime)
            .ToListAsync(ct);
    }

    public async Task<Candle?> GetLatestCandleAsync(
        Guid symbolId, Timeframe timeframe, CancellationToken ct = default)
    {
        return await _context.Candles
            .Where(c => c.SymbolId == symbolId && c.Timeframe == timeframe)
            .OrderByDescending(c => c.OpenTime)
            .FirstOrDefaultAsync(ct);
    }
}