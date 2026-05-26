using ARABAZON.TradeAI.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ARABAZON.TradeAI.API.Controllers;

[ApiController]
[Route("api/v1/risk")]
public class RiskController : ControllerBase
{
    private readonly IApplicationDbContext _context;
    private readonly IRiskEngine _riskEngine;
    private readonly IDailyRiskTracker _dailyTracker;

    public RiskController(
        IApplicationDbContext context,
        IRiskEngine riskEngine,
        IDailyRiskTracker dailyTracker)
    {
        _context = context;
        _riskEngine = riskEngine;
        _dailyTracker = dailyTracker;
    }

    // GET /api/v1/risk/configurations
    [HttpGet("configurations")]
    public async Task<IActionResult> GetConfigurations(CancellationToken ct)
    {
        var global = await _context.RiskConfigurations.FirstOrDefaultAsync(ct);
        var symbols = await _context.SymbolRiskConfigurations.ToListAsync(ct);

        return Ok(new
        {
            global = global == null ? null : new
            {
                global.MaxRiskPerTrade,
                global.DailyLossLimit,
                global.MaxConcurrentTrades,
                global.AutoTradingEnabled,
            },
            symbols = symbols.Select(s => new
            {
                s.SymbolId,
                s.MaxRiskPerTrade,
                s.MaxDailyLoss,
                s.MaxSpreadAllowed,
                s.MaxSlippageAllowed,
                s.MaxConcurrentTrades,
                s.AutoTradingEnabled,
                s.UpdatedAt,
            }),
        });
    }

    // PUT /api/v1/risk/configurations/symbol/{symbolId}
    [HttpPut("configurations/symbol/{symbolId:guid}")]
    public async Task<IActionResult> UpdateSymbolConfig(
        Guid symbolId, [FromBody] UpdateSymbolRiskRequest body, CancellationToken ct)
    {
        var config = await _context.SymbolRiskConfigurations
            .FirstOrDefaultAsync(x => x.SymbolId == symbolId, ct);

        if (config == null) return NotFound(new { error = "Symbol risk config not found" });

        config.Update(
            body.MaxRiskPerTrade,
            body.MaxDailyLoss,
            body.MaxSpreadAllowed,
            body.MaxSlippageAllowed,
            body.MaxConcurrentTrades,
            body.AutoTradingEnabled);

        await _context.SaveChangesAsync(ct);
        return Ok(new { message = "Risk configuration updated" });
    }

    // GET /api/v1/risk/exposure
    [HttpGet("exposure")]
    public async Task<IActionResult> GetExposure(CancellationToken ct)
    {
        var openTradesCount = await _context.Trades
            .CountAsync(t => t.Status == Domain.Enums.TradeStatus.Open, ct);

        var dailyDrawdown = await _riskEngine.GetDailyDrawdownAsync(ct);

        return Ok(new
        {
            openTrades = openTradesCount,
            dailyLoss = _dailyTracker.DailyLoss,
            dailyProfit = _dailyTracker.DailyProfit,
            dailyDrawdown,
            isEmergencyStop = _dailyTracker.IsEmergencyStop,
        });
    }

    // GET /api/v1/risk/events
    [HttpGet("events")]
    public async Task<IActionResult> GetRiskEvents(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var total = await _context.RiskEvents.CountAsync(ct);
        var events = await _context.RiskEvents
            .OrderByDescending(x => x.TriggeredAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new
            {
                x.Id,
                EventType = x.EventType.ToString(),
                x.Severity,
                x.Description,
                x.CurrentValue,
                x.ThresholdValue,
                x.TriggeredAt,
            })
            .ToListAsync(ct);

        return Ok(new { total, page, pageSize, data = events });
    }

    // POST /api/v1/risk/emergency-stop/clear
    [HttpPost("emergency-stop/clear")]
    public IActionResult ClearEmergencyStop()
    {
        _dailyTracker.ClearEmergencyStop();
        return Ok(new { message = "Emergency stop cleared" });
    }
}

public record UpdateSymbolRiskRequest(
    decimal MaxRiskPerTrade,
    decimal MaxDailyLoss,
    decimal MaxSpreadAllowed,
    decimal MaxSlippageAllowed,
    int MaxConcurrentTrades,
    bool AutoTradingEnabled);