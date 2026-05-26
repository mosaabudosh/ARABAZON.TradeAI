using ARABAZON.TradeAI.Application.Interfaces;
using ARABAZON.TradeAI.Domain.Entities;
using ARABAZON.TradeAI.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ARABAZON.TradeAI.Infrastructure.Services;

public class TradeExecutionService : ITradeExecutionService
{
    private readonly IApplicationDbContext _context;
    private readonly IMT5ExecutionAdapter _mt5;
    private readonly IRiskEngine _riskEngine;
    private readonly IDailyRiskTracker _dailyTracker;
    private readonly ILogger<TradeExecutionService> _logger;

    public TradeExecutionService(
        IApplicationDbContext context,
        IMT5ExecutionAdapter mt5,
        IRiskEngine riskEngine,
        IDailyRiskTracker dailyTracker,
        ILogger<TradeExecutionService> logger)
    {
        _context = context;
        _mt5 = mt5;
        _riskEngine = riskEngine;
        _dailyTracker = dailyTracker;
        _logger = logger;
    }

    public async Task<ExecuteTradeResponse> ExecuteAsync(
        ExecuteTradeRequest request, CancellationToken ct = default)
    {
        _logger.LogInformation("ExecuteAsync | SignalId={SignalId} RequestId={RequestId}",
            request.SignalId, request.RequestId);

        // 1. Load signal
        var signal = await _context.TradeSignals.FindAsync([request.SignalId], ct);
        if (signal == null)
            return ExecuteTradeResponse.Fail("SIGNAL_NOT_FOUND", "Signal not found.");

        if (signal.Status != SignalStatus.Approved)
            return ExecuteTradeResponse.Fail("SIGNAL_NOT_APPROVED",
                $"Signal status is {signal.Status}, must be Approved.");

        if (signal.IsExpired)
            return ExecuteTradeResponse.Fail("SIGNAL_EXPIRED", "Signal has expired.");

        // 2. Load symbol
        var symbol = await _context.Symbols.FindAsync([signal.SymbolId], ct);
        if (symbol == null)
            return ExecuteTradeResponse.Fail("INVALID_SYMBOL", "Symbol not found.");

        // 3. Get account info for position sizing
        MT5AccountInfo accountInfo;
        try
        {
            accountInfo = await _mt5.GetAccountInfoAsync(ct);
        }
        catch
        {
            return ExecuteTradeResponse.Fail("MT5_DISCONNECTED", "Cannot reach MT5 bridge.");
        }

        // 4. Get live spread
        decimal spread = 0;
        try
        {
            var snapshot = await LoadMarketSnapshot(symbol.SymbolCode, ct);
            spread = snapshot?.Spread ?? 0;
        }
        catch { /* non-blocking */ }

        // 5. Risk validation
        var riskRequest = new RiskValidationRequest
        {
            SymbolId = signal.SymbolId,
            SymbolCode = symbol.SymbolCode,
            SignalType = signal.SignalType.ToString(),
            EntryPrice = signal.EntryPrice.Value,
            StopLoss = signal.StopLoss.Value,
            TakeProfit = signal.TakeProfit.Value,
            AccountBalance = accountInfo.Balance,
            CurrentSpread = spread,
        };

        var riskResult = await _riskEngine.ValidateAsync(riskRequest, ct);
        if (!riskResult.IsApproved)
        {
            await WriteAuditLog(request.RequestId, request.CorrelationId,
                "Open", "RejectedByRisk", null, null,
                signal.EntryPrice.Value, null, null, riskResult.RejectionReason, ct);

            return ExecuteTradeResponse.Fail(
                riskResult.RejectionCode ?? "RISK_REJECTED",
                riskResult.RejectionReason ?? "Risk validation failed.");
        }

        // 6. Send to MT5
        var mt5Request = new MT5OpenRequest
        {
            RequestId = request.RequestId,
            CorrelationId = request.CorrelationId,
            IdempotencyKey = request.IdempotencyKey,
            Symbol = symbol.SymbolCode,
            TradeType = signal.SignalType.ToString(),
            Volume = riskResult.PositionSize,
            EntryPrice = signal.EntryPrice.Value,
            StopLoss = signal.StopLoss.Value,
            TakeProfit = signal.TakeProfit.Value,
        };

        var mt5Result = await _mt5.OpenTradeAsync(mt5Request, ct);

        if (!mt5Result.Success)
        {
            await WriteAuditLog(request.RequestId, request.CorrelationId,
                "Open", "Failed", null, null,
                signal.EntryPrice.Value, null, null, mt5Result.ErrorMessage, ct);

            return ExecuteTradeResponse.Fail("MT5_EXECUTION_FAILED",
                mt5Result.ErrorMessage ?? "MT5 execution failed.");
        }

        // 7. Persist Trade
        var tradeType = signal.SignalType == SignalType.Buy ? TradeType.Buy : TradeType.Sell;
        var trade = Trade.Create(
            signal.Id, symbol.Id, tradeType,
            mt5Result.ExecutedPrice, signal.StopLoss.Value,
            signal.TakeProfit.Value, riskResult.PositionSize);

        trade.Open(mt5Result.BrokerTicket!);

        await _context.Trades.AddAsync(trade, ct);

        // 8. Persist TradeExecution record
        var execution = TradeExecution.Create(
            trade.Id, "Open", signal.EntryPrice.Value,
            request.RequestId, request.CorrelationId);
        execution.MarkSuccess(mt5Result.ExecutedPrice, mt5Result.BrokerTicket!);
        await _context.TradeExecutions.AddAsync(execution, ct);

        // 9. Create Position snapshot
        var position = Position.Create(trade.Id, mt5Result.ExecutedPrice, riskResult.PositionSize);
        await _context.Positions.AddAsync(position, ct);

        // 10. Mark signal as executed
        signal.MarkAsExecuted();

        await _context.SaveChangesAsync(ct);

        // 11. Audit log
        await WriteAuditLog(request.RequestId, request.CorrelationId,
            "Open", "Success", trade.Id, mt5Result.BrokerTicket,
            signal.EntryPrice.Value, mt5Result.ExecutedPrice, mt5Result.Slippage, null, ct);

        _logger.LogInformation(
            "Trade executed | TradeId={TradeId} Ticket={Ticket} Price={Price} Lots={Lots}",
            trade.Id, mt5Result.BrokerTicket, mt5Result.ExecutedPrice, riskResult.PositionSize);

        return ExecuteTradeResponse.Ok(
            trade.Id, mt5Result.BrokerTicket!,
            mt5Result.ExecutedPrice, riskResult.PositionSize);
    }

    public async Task<ExecuteTradeResponse> CloseAsync(
        CloseTradeRequest request, CancellationToken ct = default)
    {
        _logger.LogInformation("CloseAsync | TradeId={TradeId}", request.TradeId);

        var trade = await _context.Trades.FindAsync([request.TradeId], ct);
        if (trade == null)
            return ExecuteTradeResponse.Fail("TRADE_NOT_FOUND", "Trade not found.");

        if (trade.Status != TradeStatus.Open)
            return ExecuteTradeResponse.Fail("TRADE_NOT_OPEN",
                $"Trade status is {trade.Status}.");

        if (string.IsNullOrEmpty(trade.BrokerTicket))
            return ExecuteTradeResponse.Fail("NO_BROKER_TICKET", "No broker ticket on trade.");

        var mt5Request = new MT5CloseRequest
        {
            RequestId = request.RequestId,
            CorrelationId = request.CorrelationId,
            Ticket = trade.BrokerTicket,
        };

        var mt5Result = await _mt5.CloseTradeAsync(mt5Request, ct);

        if (!mt5Result.Success)
        {
            await WriteAuditLog(request.RequestId, request.CorrelationId,
                "Close", "Failed", trade.Id, trade.BrokerTicket,
                null, null, null, mt5Result.ErrorMessage, ct);

            return ExecuteTradeResponse.Fail("MT5_CLOSE_FAILED",
                mt5Result.ErrorMessage ?? "MT5 close failed.");
        }

        // Calculate P&L
        decimal pnl = trade.TradeType == TradeType.Buy
            ? (mt5Result.ExecutedPrice - trade.EntryPrice.Value) * trade.PositionSize
            : (trade.EntryPrice.Value - mt5Result.ExecutedPrice) * trade.PositionSize;

        trade.Close(mt5Result.ExecutedPrice, pnl);
        _dailyTracker.RecordTrade(pnl);

        // Execution record
        var execution = TradeExecution.Create(
            trade.Id, "Close", trade.EntryPrice.Value,
            request.RequestId, request.CorrelationId);
        execution.MarkSuccess(mt5Result.ExecutedPrice, trade.BrokerTicket);
        await _context.TradeExecutions.AddAsync(execution, ct);

        await _context.SaveChangesAsync(ct);

        await WriteAuditLog(request.RequestId, request.CorrelationId,
            "Close", "Success", trade.Id, trade.BrokerTicket,
            trade.EntryPrice.Value, mt5Result.ExecutedPrice, mt5Result.Slippage, null, ct);

        _logger.LogInformation("Trade closed | TradeId={TradeId} PnL={PnL}", trade.Id, pnl);

        return ExecuteTradeResponse.Ok(trade.Id, trade.BrokerTicket!, mt5Result.ExecutedPrice, 0);
    }

    private async Task<MarketSnapshot?> LoadMarketSnapshot(string symbolCode, CancellationToken ct)
    {
        var symbol = await _context.Symbols.FirstOrDefaultAsync(s => s.SymbolCode == symbolCode, ct);
        if (symbol == null) return null;
        return await _context.MarketSnapshots
            .Where(s => s.SymbolId == symbol.Id)
            .OrderByDescending(s => s.SnapshotTime)
            .FirstOrDefaultAsync(ct);
    }

    private async Task WriteAuditLog(
        string requestId, string correlationId, string action, string status,
        Guid? tradeId, string? ticket, decimal? reqPrice, decimal? execPrice,
        decimal? slippage, string? error, CancellationToken ct)
    {
        var log = ExecutionAuditLog.Create(requestId, correlationId, action, status,
            tradeId, ticket, reqPrice, execPrice, slippage, error);
        await _context.ExecutionAuditLogs.AddAsync(log, ct);
        await _context.SaveChangesAsync(ct);
    }
}