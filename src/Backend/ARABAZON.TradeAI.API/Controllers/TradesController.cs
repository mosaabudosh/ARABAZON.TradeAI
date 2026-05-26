using ARABAZON.TradeAI.Application.Interfaces;
using ARABAZON.TradeAI.Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ARABAZON.TradeAI.API.Controllers;

[ApiController]
[Route("api/v1/trades")]
public class TradesController : ControllerBase
{
    private readonly IApplicationDbContext _context;
    private readonly ITradeExecutionService _executionService;

    public TradesController(
        IApplicationDbContext context,
        ITradeExecutionService executionService)
    {
        _context = context;
        _executionService = executionService;
    }

    // GET /api/v1/trades/open
    [HttpGet("open")]
    public async Task<IActionResult> GetOpenTrades(CancellationToken ct)
    {
        var trades = await _context.Trades
            .Where(t => t.Status == TradeStatus.Open)
            .Select(t => new
            {
                t.Id,
                t.SymbolId,
                t.BrokerTicket,
                TradeType = t.TradeType.ToString(),
                EntryPrice = t.EntryPrice.Value,
                StopLoss = t.StopLoss.Value,
                TakeProfit = t.TakeProfit.Value,
                t.PositionSize,
                Status = t.Status.ToString(),
                t.OpenedAt,
            })
            .ToListAsync(ct);

        return Ok(trades);
    }

    // GET /api/v1/trades/history
    [HttpGet("history")]
    public async Task<IActionResult> GetTradeHistory(
        [FromQuery] string? symbol = null,
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var query = _context.Trades
            .Where(t => t.Status == TradeStatus.Closed);

        if (from.HasValue) query = query.Where(t => t.OpenedAt >= from.Value);
        if (to.HasValue) query = query.Where(t => t.OpenedAt <= to.Value);

        var total = await query.CountAsync(ct);

        var trades = await query
            .OrderByDescending(t => t.ClosedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(t => new
            {
                t.Id,
                t.SymbolId,
                t.BrokerTicket,
                TradeType = t.TradeType.ToString(),
                EntryPrice = t.EntryPrice.Value,
                ExitPrice = t.ExitPrice != null ? t.ExitPrice.Value : (decimal?)null,
                StopLoss = t.StopLoss.Value,
                TakeProfit = t.TakeProfit.Value,
                t.PositionSize,
                ProfitLoss = t.ProfitLoss.Amount,
                Status = t.Status.ToString(),
                t.OpenedAt,
                t.ClosedAt,
            })
            .ToListAsync(ct);

        return Ok(new { total, page, pageSize, data = trades });
    }

    // POST /api/v1/trades/manual-execute
    [HttpPost("manual-execute")]
    public async Task<IActionResult> ManualExecute(
        [FromBody] ManualExecuteRequest body, CancellationToken ct)
    {
        var request = new ExecuteTradeRequest
        {
            SignalId = body.SignalId,
            RequestId = body.RequestId ?? Guid.NewGuid().ToString(),
            CorrelationId = body.CorrelationId ?? Guid.NewGuid().ToString(),
            IdempotencyKey = body.IdempotencyKey ?? Guid.NewGuid().ToString(),
            IsManual = true,
        };

        var result = await _executionService.ExecuteAsync(request, ct);

        if (!result.Success)
            return BadRequest(new { error = result.ErrorCode, message = result.ErrorMessage });

        return Ok(new
        {
            tradeId = result.TradeId,
            brokerTicket = result.BrokerTicket,
            executedPrice = result.ExecutedPrice,
            positionSize = result.PositionSize,
        });
    }

    // POST /api/v1/trades/close
    [HttpPost("close")]
    public async Task<IActionResult> CloseTrade(
        [FromBody] CloseTradeRequestDto body, CancellationToken ct)
    {
        var request = new CloseTradeRequest
        {
            TradeId = body.TradeId,
            RequestId = Guid.NewGuid().ToString(),
            CorrelationId = Guid.NewGuid().ToString(),
            Reason = body.Reason ?? "Manual",
        };

        var result = await _executionService.CloseAsync(request, ct);

        if (!result.Success)
            return BadRequest(new { error = result.ErrorCode, message = result.ErrorMessage });

        return Ok(new { message = "Trade closed", tradeId = body.TradeId });
    }

    // GET /api/v1/trades/{id}
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetTrade(Guid id, CancellationToken ct)
    {
        var trade = await _context.Trades
            .Where(t => t.Id == id)
            .Select(t => new
            {
                t.Id,
                t.SignalId,
                t.BrokerTicket,
                t.SymbolId,
                TradeType = t.TradeType.ToString(),
                EntryPrice = t.EntryPrice.Value,
                ExitPrice = t.ExitPrice != null ? t.ExitPrice.Value : (decimal?)null,
                StopLoss = t.StopLoss.Value,
                TakeProfit = t.TakeProfit.Value,
                t.PositionSize,
                ProfitLoss = t.ProfitLoss.Amount,
                Status = t.Status.ToString(),
                t.OpenedAt,
                t.ClosedAt,
            })
            .FirstOrDefaultAsync(ct);

        if (trade == null) return NotFound();
        return Ok(trade);
    }
}

public record ManualExecuteRequest(
    Guid SignalId,
    string? RequestId,
    string? CorrelationId,
    string? IdempotencyKey);

public record CloseTradeRequestDto(Guid TradeId, string? Reason);