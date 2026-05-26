namespace ARABAZON.TradeAI.Application.Interfaces;

public class RiskValidationRequest
{
    public Guid SymbolId { get; init; }
    public string SymbolCode { get; init; } = default!;
    public string SignalType { get; init; } = default!;
    public decimal EntryPrice { get; init; }
    public decimal StopLoss { get; init; }
    public decimal TakeProfit { get; init; }
    public decimal AccountBalance { get; init; }
    public decimal CurrentSpread { get; init; }
}

public class RiskValidationResult
{
    public bool IsApproved { get; init; }
    public decimal PositionSize { get; init; }
    public decimal RiskAmount { get; init; }
    public decimal RiskPercentage { get; init; }
    public string? RejectionReason { get; init; }
    public string? RejectionCode { get; init; }

    public static RiskValidationResult Approved(
        decimal positionSize, decimal riskAmount, decimal riskPercentage) => new()
        {
            IsApproved = true,
            PositionSize = positionSize,
            RiskAmount = riskAmount,
            RiskPercentage = riskPercentage,
        };

    public static RiskValidationResult Rejected(string reason, string code) => new()
    {
        IsApproved = false,
        RejectionReason = reason,
        RejectionCode = code,
    };
}

public interface IRiskEngine
{
    Task<RiskValidationResult> ValidateAsync(RiskValidationRequest request, CancellationToken ct = default);
    Task<bool> CanOpenNewTradeAsync(Guid symbolId, CancellationToken ct = default);
    Task<decimal> GetDailyDrawdownAsync(CancellationToken ct = default);
    Task<decimal> GetCurrentExposureAsync(CancellationToken ct = default);
}