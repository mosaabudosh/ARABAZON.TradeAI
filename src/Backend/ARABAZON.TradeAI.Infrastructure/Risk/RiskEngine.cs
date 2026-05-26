using ARABAZON.TradeAI.Application.Interfaces;
using ARABAZON.TradeAI.Domain.Entities;
using ARABAZON.TradeAI.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ARABAZON.TradeAI.Infrastructure.Risk;

public class RiskEngine : IRiskEngine
{
    private readonly IApplicationDbContext _context;
    private readonly IPositionSizingService _positionSizing;
    private readonly IDailyRiskTracker _dailyTracker;
    private readonly ILogger<RiskEngine> _logger;

    // Contract sizes
    private static readonly Dictionary<string, (decimal ContractSize, decimal TickSize)> SymbolSpecs = new()
    {
        ["XAUUSD"] = (100m, 0.01m),
        ["USOIL"] = (1000m, 0.01m),
    };

    public RiskEngine(
        IApplicationDbContext context,
        IPositionSizingService positionSizing,
        IDailyRiskTracker dailyTracker,
        ILogger<RiskEngine> logger)
    {
        _context = context;
        _positionSizing = positionSizing;
        _dailyTracker = dailyTracker;
        _logger = logger;
    }

    public async Task<RiskValidationResult> ValidateAsync(
        RiskValidationRequest request, CancellationToken ct = default)
    {
        _logger.LogInformation("Validating risk for {Symbol} {SignalType}",
            request.SymbolCode, request.SignalType);

        // 1. Emergency stop check
        if (_dailyTracker.IsEmergencyStop)
            return RiskValidationResult.Rejected(
                "Emergency stop is active — no new trades allowed.",
                "EMERGENCY_STOP");

        // 2. Load symbol risk config
        var riskConfig = await _context.SymbolRiskConfigurations
            .FirstOrDefaultAsync(x => x.SymbolId == request.SymbolId, ct);

        if (riskConfig == null)
            return RiskValidationResult.Rejected(
                "No risk configuration found for symbol.",
                "NO_RISK_CONFIG");

        // 3. Auto trading check
        if (!riskConfig.AutoTradingEnabled)
            return RiskValidationResult.Rejected(
                "Auto trading is disabled for this symbol.",
                "AUTO_TRADING_DISABLED");

        // 4. Spread check
        if (request.CurrentSpread > riskConfig.MaxSpreadAllowed)
        {
            await LogRiskEventAsync(RiskEventType.SpreadExplosion, "High",
                $"Spread {request.CurrentSpread} exceeds max {riskConfig.MaxSpreadAllowed}",
                request.SymbolId, request.CurrentSpread, riskConfig.MaxSpreadAllowed, ct);

            return RiskValidationResult.Rejected(
                $"Spread too high: {request.CurrentSpread} > {riskConfig.MaxSpreadAllowed}",
                "SPREAD_TOO_HIGH");
        }

        // 5. Daily loss limit check
        if (request.AccountBalance > 0)
        {
            decimal dailyLossPct = (_dailyTracker.DailyLoss / request.AccountBalance) * 100m;
            if (dailyLossPct >= riskConfig.MaxDailyLoss)
            {
                await LogRiskEventAsync(RiskEventType.DailyLossLimitReached, "Critical",
                    $"Daily loss {dailyLossPct:F2}% reached limit {riskConfig.MaxDailyLoss}%",
                    request.SymbolId, dailyLossPct, riskConfig.MaxDailyLoss, ct);

                _dailyTracker.TriggerEmergencyStop("Daily loss limit reached");
                return RiskValidationResult.Rejected(
                    $"Daily loss limit reached: {dailyLossPct:F2}% >= {riskConfig.MaxDailyLoss}%",
                    "RISK_LIMIT_EXCEEDED");
            }
        }

        // 6. Concurrent trades check
        var openTrades = await _context.Trades
            .CountAsync(t => t.SymbolId == request.SymbolId &&
                             t.Status == TradeStatus.Open, ct);

        if (openTrades >= riskConfig.MaxConcurrentTrades)
            return RiskValidationResult.Rejected(
                $"Max concurrent trades reached: {openTrades}/{riskConfig.MaxConcurrentTrades}",
                "MAX_TRADES_REACHED");

        // 7. SL/TP validation
        if (request.StopLoss <= 0 || request.TakeProfit <= 0)
            return RiskValidationResult.Rejected("Invalid SL or TP value.", "INVALID_STOP_LOSS");

        bool isBuy = request.SignalType == "Buy";
        if (isBuy && request.StopLoss >= request.EntryPrice)
            return RiskValidationResult.Rejected("SL must be below entry for Buy.", "INVALID_STOP_LOSS");
        if (!isBuy && request.StopLoss <= request.EntryPrice)
            return RiskValidationResult.Rejected("SL must be above entry for Sell.", "INVALID_STOP_LOSS");

        // 8. Calculate position size
        if (!SymbolSpecs.TryGetValue(request.SymbolCode, out var specs))
            return RiskValidationResult.Rejected("Unknown symbol specs.", "INVALID_SYMBOL");

        decimal positionSize = _positionSizing.Calculate(
            request.AccountBalance,
            riskConfig.MaxRiskPerTrade,
            request.EntryPrice,
            request.StopLoss,
            specs.ContractSize,
            specs.TickSize);

        if (positionSize > riskConfig.MaxPositionSize)
            positionSize = riskConfig.MaxPositionSize;

        decimal riskAmount = Math.Abs(request.EntryPrice - request.StopLoss)
                              * positionSize * specs.ContractSize;
        decimal riskPct = request.AccountBalance > 0
                              ? (riskAmount / request.AccountBalance) * 100m
                              : 0;

        _logger.LogInformation(
            "Risk approved: {Symbol} Lots={Lots} Risk={Risk}% Amount={Amount}",
            request.SymbolCode, positionSize, riskPct, riskAmount);

        return RiskValidationResult.Approved(positionSize, riskAmount, riskPct);
    }

    public async Task<bool> CanOpenNewTradeAsync(Guid symbolId, CancellationToken ct = default)
    {
        if (_dailyTracker.IsEmergencyStop) return false;

        var config = await _context.SymbolRiskConfigurations
            .FirstOrDefaultAsync(x => x.SymbolId == symbolId, ct);
        if (config == null || !config.AutoTradingEnabled) return false;

        var openCount = await _context.Trades
            .CountAsync(t => t.SymbolId == symbolId && t.Status == TradeStatus.Open, ct);

        return openCount < config.MaxConcurrentTrades;
    }

    public async Task<decimal> GetDailyDrawdownAsync(CancellationToken ct = default)
        => await Task.FromResult(_dailyTracker.DailyLoss);

    public async Task<decimal> GetCurrentExposureAsync(CancellationToken ct = default)
    {
        var openTrades = await _context.Trades
            .Where(t => t.Status == TradeStatus.Open)
            .CountAsync(ct);
        return openTrades;
    }

    private async Task LogRiskEventAsync(
        RiskEventType eventType, string severity, string description,
        Guid? symbolId, decimal? current, decimal? threshold,
        CancellationToken ct)
    {
        var riskEvent = RiskEvent.Create(eventType, severity, description,
            symbolId, current, threshold);
        await _context.RiskEvents.AddAsync(riskEvent, ct);
        await _context.SaveChangesAsync(ct);
    }
}