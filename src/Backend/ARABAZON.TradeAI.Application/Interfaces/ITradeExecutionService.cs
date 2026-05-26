namespace ARABAZON.TradeAI.Application.Interfaces;

public class ExecuteTradeRequest
{
    public Guid SignalId { get; init; }
    public string RequestId { get; init; } = Guid.NewGuid().ToString();
    public string CorrelationId { get; init; } = Guid.NewGuid().ToString();
    public string IdempotencyKey { get; init; } = Guid.NewGuid().ToString();
    public bool IsManual { get; init; }
}

public class ExecuteTradeResponse
{
    public bool Success { get; init; }
    public Guid? TradeId { get; init; }
    public string? BrokerTicket { get; init; }
    public decimal? ExecutedPrice { get; init; }
    public decimal? PositionSize { get; init; }
    public string? ErrorCode { get; init; }
    public string? ErrorMessage { get; init; }

    public static ExecuteTradeResponse Ok(Guid tradeId, string brokerTicket,
        decimal executedPrice, decimal positionSize) => new()
        {
            Success = true,
            TradeId = tradeId,
            BrokerTicket = brokerTicket,
            ExecutedPrice = executedPrice,
            PositionSize = positionSize,
        };

    public static ExecuteTradeResponse Fail(string code, string message) => new()
    {
        Success = false,
        ErrorCode = code,
        ErrorMessage = message,
    };
}

public class CloseTradeRequest
{
    public Guid TradeId { get; init; }
    public string RequestId { get; init; } = Guid.NewGuid().ToString();
    public string CorrelationId { get; init; } = Guid.NewGuid().ToString();
    public string Reason { get; init; } = "Manual";
}

public interface ITradeExecutionService
{
    Task<ExecuteTradeResponse> ExecuteAsync(ExecuteTradeRequest request, CancellationToken ct = default);
    Task<ExecuteTradeResponse> CloseAsync(CloseTradeRequest request, CancellationToken ct = default);
}