namespace ARABAZON.TradeAI.Domain.Entities;

public class ExecutionAuditLog
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string RequestId { get; private set; } = default!;
    public string CorrelationId { get; private set; } = default!;
    public Guid? TradeId { get; private set; }
    public string? BrokerTicket { get; private set; }
    public string ExecutionAction { get; private set; } = default!;
    public string ExecutionStatus { get; private set; } = default!;
    public decimal? RequestedPrice { get; private set; }
    public decimal? ExecutedPrice { get; private set; }
    public decimal? Slippage { get; private set; }
    public string? ErrorMessage { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private ExecutionAuditLog() { }

    public static ExecutionAuditLog Create(
        string requestId, string correlationId,
        string action, string status,
        Guid? tradeId = null, string? brokerTicket = null,
        decimal? requestedPrice = null, decimal? executedPrice = null,
        decimal? slippage = null, string? errorMessage = null) => new()
        {
            RequestId = requestId,
            CorrelationId = correlationId,
            TradeId = tradeId,
            BrokerTicket = brokerTicket,
            ExecutionAction = action,
            ExecutionStatus = status,
            RequestedPrice = requestedPrice,
            ExecutedPrice = executedPrice,
            Slippage = slippage,
            ErrorMessage = errorMessage,
            CreatedAt = DateTime.UtcNow,
        };
}