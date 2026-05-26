using ARABAZON.TradeAI.Application.Interfaces;
using ARABAZON.TradeAI.Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ARABAZON.TradeAI.API.Controllers;

[ApiController]
[Route("api/v1/market")]
public class MarketController : ControllerBase
{
    private readonly IApplicationDbContext _context;
    private readonly IMarketDataService _marketDataService;
    private readonly ICandleRepository _candleRepository;

    public MarketController(
        IApplicationDbContext context,
        IMarketDataService marketDataService,
        ICandleRepository candleRepository)
    {
        _context = context;
        _marketDataService = marketDataService;
        _candleRepository = candleRepository;
    }

    [HttpGet("symbols")]
    public async Task<IActionResult> GetSymbols(CancellationToken ct)
    {
        var symbols = await _context.Symbols
            .Where(s => s.IsActive)
            .Select(s => new { s.SymbolCode, s.Name, MarketType = s.MarketType.ToString() })
            .ToListAsync(ct);

        return Ok(symbols);
    }

    [HttpGet("candles")]
    public async Task<IActionResult> GetCandles(
        [FromQuery] string symbol,
        [FromQuery] string timeframe = "M5",
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null,
        CancellationToken ct = default)
    {
        if (!Enum.TryParse<Timeframe>(timeframe, true, out var tf))
            return BadRequest(new { error = "INVALID_TIMEFRAME" });

        var sym = await _context.Symbols.FirstOrDefaultAsync(s => s.SymbolCode == symbol, ct);
        if (sym == null) return NotFound(new { error = "INVALID_SYMBOL" });

        var fromDate = from ?? DateTime.UtcNow.AddDays(-1);
        var toDate = to ?? DateTime.UtcNow;

        var candles = await _candleRepository.GetCandlesAsync(sym.Id, tf, fromDate, toDate, ct);

        var result = candles.Select(c => new
        {
            c.OpenPrice.Value,
            HighPrice = c.HighPrice.Value,
            LowPrice = c.LowPrice.Value,
            ClosePrice = c.ClosePrice.Value,
            c.Volume,
            c.OpenTime,
            c.CloseTime,
            IsBullish = c.IsBullish
        });

        return Ok(result);
    }

    [HttpGet("live")]
    public async Task<IActionResult> GetLive([FromQuery] string symbol, CancellationToken ct)
    {
        try
        {
            var snapshot = await _marketDataService.GetLiveSnapshotAsync(symbol, ct);
            return Ok(new
            {
                symbol,
                bid = snapshot.BidPrice,
                ask = snapshot.AskPrice,
                spread = snapshot.Spread,
                midPrice = snapshot.MidPrice,
                timestamp = snapshot.SnapshotTime
            });
        }
        catch
        {
            return StatusCode(503, new { error = "MT5_DISCONNECTED" });
        }
    }
}