namespace ARABAZON.TradeAI.Application.Interfaces;

public class SignalNotification
{
    public Guid SignalId { get; init; }
    public string Symbol { get; init; } = default!;
    public string SignalType { get; init; } = default!;
    public decimal EntryPrice { get; init; }
    public decimal StopLoss { get; init; }
    public decimal TakeProfit { get; init; }
    public decimal ConfidenceScore { get; init; }
    public string Reason { get; init; } = default!;
    public DateTime GeneratedAt { get; init; }
}

public interface ISignalHubNotifier
{
    Task BroadcastSignalAsync(SignalNotification notification);
}