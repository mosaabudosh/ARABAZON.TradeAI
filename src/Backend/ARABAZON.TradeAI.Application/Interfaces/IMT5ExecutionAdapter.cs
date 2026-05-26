namespace ARABAZON.TradeAI.Application.Interfaces;

public class MT5OpenRequest
{
    public string RequestId { get; init; } = default!;
    public string CorrelationId { get; init; } = default!;
    public string IdempotencyKey { get; init; } = default!;
    public string Symbol { get; init; } = default!;
    public string TradeType { get; init; } = default!;
    public decimal Volume { get; init; }
    public decimal EntryPrice { get; init; }
    public decimal StopLoss { get; init; }
    public decimal TakeProfit { get; init; }
}

public class MT5CloseRequest
{
    public string RequestId { get; init; } = default!;
    public string CorrelationId { get; init; } = default!;
    public string Ticket { get; init; } = default!;
}

public class MT5TradeResult
{
    public bool Success { get; init; }
    public string? BrokerTicket { get; init; }
    public decimal ExecutedPrice { get; init; }
    public decimal Slippage { get; init; }
    public string? ErrorMessage { get; init; }
}

public class MT5PositionInfo
{
    public string Ticket { get; init; } = default!;
    public string Symbol { get; init; } = default!;
    public string TradeType { get; init; } = default!;
    public decimal Volume { get; init; }
    public decimal OpenPrice { get; init; }
    public decimal CurrentPrice { get; init; }
    public decimal StopLoss { get; init; }
    public decimal TakeProfit { get; init; }
    public decimal Profit { get; init; }
}

public class MT5AccountInfo
{
    public decimal Balance { get; init; }
    public decimal Equity { get; init; }
    public decimal Margin { get; init; }
    public decimal FreeMargin { get; init; }
    public decimal MarginLevel { get; init; }
    public string Currency { get; init; } = "USD";
}

public interface IMT5ExecutionAdapter
{
    Task<MT5TradeResult> OpenTradeAsync(MT5OpenRequest request, CancellationToken ct = default);
    Task<MT5TradeResult> CloseTradeAsync(MT5CloseRequest request, CancellationToken ct = default);
    Task<IEnumerable<MT5PositionInfo>> GetPositionsAsync(CancellationToken ct = default);
    Task<MT5AccountInfo> GetAccountInfoAsync(CancellationToken ct = default);
    Task<bool> IsConnectedAsync(CancellationToken ct = default);
}