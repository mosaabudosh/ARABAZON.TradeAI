using ARABAZON.TradeAI.Application.Interfaces;
using ARABAZON.TradeAI.Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ARABAZON.TradeAI.API.Controllers;

[ApiController]
[Route("api/v1/signals")]
public class SignalsController : ControllerBase
{
    private readonly IApplicationDbContext _context;

    public SignalsController(IApplicationDbContext context) => _context = context;

    [HttpGet]
    public async Task<IActionResult> GetSignals(
        [FromQuery] string? status = null,
        [FromQuery] string? symbol = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var query = _context.TradeSignals.AsQueryable();

        if (!string.IsNullOrEmpty(status) && Enum.TryParse<SignalStatus>(status, true, out var s))
            query = query.Where(x => x.Status == s);

        if (!string.IsNullOrEmpty(symbol))
        {
            var sym = await _context.Symbols.FirstOrDefaultAsync(x => x.SymbolCode == symbol, ct);
            if (sym != null) query = query.Where(x => x.SymbolId == sym.Id);
        }

        var total = await query.CountAsync(ct);

        var signals = await query
            .OrderByDescending(x => x.GeneratedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new
            {
                x.Id,
                x.SymbolId,
                x.StrategyName,
                SignalType = x.SignalType.ToString(),
                EntryPrice = x.EntryPrice.Value,
                StopLoss = x.StopLoss.Value,
                TakeProfit = x.TakeProfit.Value,
                ConfidenceScore = x.ConfidenceScore.Value,
                RiskPercentage = x.RiskPercentage.Value,
                Status = x.Status.ToString(),
                x.GeneratedAt,
                x.ExpiresAt,
            })
            .ToListAsync(ct);

        return Ok(new { total, page, pageSize, data = signals });
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetSignal(Guid id, CancellationToken ct)
    {
        var signal = await _context.TradeSignals
            .Where(x => x.Id == id)
            .Select(x => new
            {
                x.Id,
                x.SymbolId,
                x.StrategyName,
                SignalType = x.SignalType.ToString(),
                EntryPrice = x.EntryPrice.Value,
                StopLoss = x.StopLoss.Value,
                TakeProfit = x.TakeProfit.Value,
                ConfidenceScore = x.ConfidenceScore.Value,
                RiskPercentage = x.RiskPercentage.Value,
                Status = x.Status.ToString(),
                x.GeneratedAt,
                x.ExpiresAt,
            })
            .FirstOrDefaultAsync(ct);

        if (signal == null) return NotFound(new { error = "Signal not found" });
        return Ok(signal);
    }

    [HttpPatch("{id:guid}/approve")]
    public async Task<IActionResult> ApproveSignal(Guid id, CancellationToken ct)
    {
        var signal = await _context.TradeSignals.FindAsync([id], ct);
        if (signal == null) return NotFound();

        try
        {
            signal.Approve();
            await _context.SaveChangesAsync(ct);
            return Ok(new { message = "Signal approved", signalId = id });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPatch("{id:guid}/reject")]
    public async Task<IActionResult> RejectSignal(Guid id, [FromBody] RejectRequest body, CancellationToken ct)
    {
        var signal = await _context.TradeSignals.FindAsync([id], ct);
        if (signal == null) return NotFound();

        signal.Reject(body.Reason);
        await _context.SaveChangesAsync(ct);
        return Ok(new { message = "Signal rejected", signalId = id });
    }

    public record RejectRequest(string Reason);
}