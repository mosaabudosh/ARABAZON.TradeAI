using ARABAZON.TradeAI.Domain.Enums;
using ARABAZON.TradeAI.Domain.ValueObjects;

namespace ARABAZON.TradeAI.Domain.Entities;

public class TradeExecution
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid TradeId { get; private set; }
    public string ExecutionType { get; private set; } = default!;  // Open / Close / Modify
    public Price RequestedPrice { get; private set; } = default!;
    public Price? ExecutedPrice { get; private set; }
    public decimal Slippage { get; private set; }
    public ExecutionStatus Status { get; private set; }
    public string? ErrorMessage { get; private set; }
    public string? BrokerTicket { get; private set; }
    public string RequestId { get; private set; } = default!;
    public string CorrelationId { get; private set; } = default!;
    public DateTime ExecutedAt { get; private set; }

    private TradeExecution() { }

    public static TradeExecution Create(
        Guid tradeId, string executionType,
        decimal requestedPrice, string requestId, string correlationId) => new()
        {
            TradeId = tradeId,
            ExecutionType = executionType,
            RequestedPrice = new Price(requestedPrice),
            Status = ExecutionStatus.Pending,
            RequestId = requestId,
            CorrelationId = correlationId,
            ExecutedAt = DateTime.UtcNow,
        };

    public void MarkSuccess(decimal executedPrice, string brokerTicket)
    {
        ExecutedPrice = new Price(executedPrice);
        Slippage = Math.Round(Math.Abs(executedPrice - RequestedPrice.Value), 5);
        BrokerTicket = brokerTicket;
        Status = ExecutionStatus.Success;
    }

    public void MarkFailed(string errorMessage)
    {
        ErrorMessage = errorMessage;
        Status = ExecutionStatus.Failed;
    }
}