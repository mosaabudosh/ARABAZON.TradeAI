using System.Net.Http.Json;
using ARABAZON.TradeAI.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace ARABAZON.TradeAI.Infrastructure.Brokers;

public class MT5ExecutionAdapter : IMT5ExecutionAdapter
{
    private readonly HttpClient _http;
    private readonly ILogger<MT5ExecutionAdapter> _logger;

    public MT5ExecutionAdapter(HttpClient http, ILogger<MT5ExecutionAdapter> logger)
    {
        _http = http;
        _logger = logger;
    }

    public async Task<MT5TradeResult> OpenTradeAsync(MT5OpenRequest request, CancellationToken ct = default)
    {
        try
        {
            var payload = new
            {
                request_id = request.RequestId,
                correlation_id = request.CorrelationId,
                idempotency_key = request.IdempotencyKey,
                symbol = request.Symbol,
                trade_type = request.TradeType,
                volume = request.Volume,
                entry_price = request.EntryPrice,
                stop_loss = request.StopLoss,
                take_profit = request.TakeProfit,
            };

            var response = await _http.PostAsJsonAsync("/api/v1/mt5/trade/open", payload, ct);
            var result = await response.Content.ReadFromJsonAsync<MT5BridgeResponse<MT5TradeData>>(ct);

            if (result?.Success != true || result.Data == null)
            {
                var error = result?.Errors?.FirstOrDefault() ?? "Unknown MT5 error";
                _logger.LogError("MT5 open failed: {Error}", error);
                return new MT5TradeResult { Success = false, ErrorMessage = error };
            }

            return new MT5TradeResult
            {
                Success = result.Data.ExecutionStatus == "Success",
                BrokerTicket = result.Data.BrokerTicket,
                ExecutedPrice = (decimal)result.Data.ExecutedPrice,
                Slippage = (decimal)result.Data.Slippage,
                ErrorMessage = result.Data.ErrorMessage,
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "MT5 open trade exception");
            return new MT5TradeResult { Success = false, ErrorMessage = ex.Message };
        }
    }

    public async Task<MT5TradeResult> CloseTradeAsync(MT5CloseRequest request, CancellationToken ct = default)
    {
        try
        {
            var payload = new
            {
                request_id = request.RequestId,
                correlation_id = request.CorrelationId,
                ticket = request.Ticket,
            };

            var response = await _http.PostAsJsonAsync("/api/v1/mt5/trade/close", payload, ct);
            var result = await response.Content.ReadFromJsonAsync<MT5BridgeResponse<MT5TradeData>>(ct);

            if (result?.Success != true || result.Data == null)
            {
                var error = result?.Errors?.FirstOrDefault() ?? "Unknown MT5 error";
                return new MT5TradeResult { Success = false, ErrorMessage = error };
            }

            return new MT5TradeResult
            {
                Success = result.Data.ExecutionStatus == "Success",
                BrokerTicket = result.Data.BrokerTicket,
                ExecutedPrice = (decimal)result.Data.ExecutedPrice,
                ErrorMessage = result.Data.ErrorMessage,
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "MT5 close trade exception");
            return new MT5TradeResult { Success = false, ErrorMessage = ex.Message };
        }
    }

    public async Task<IEnumerable<MT5PositionInfo>> GetPositionsAsync(CancellationToken ct = default)
    {
        try
        {
            var response = await _http.GetFromJsonAsync<MT5BridgeResponse<List<MT5PositionData>>>(
                "/api/v1/mt5/positions", ct);

            if (response?.Success != true || response.Data == null)
                return Enumerable.Empty<MT5PositionInfo>();

            return response.Data.Select(p => new MT5PositionInfo
            {
                Ticket = p.Ticket,
                Symbol = p.Symbol,
                TradeType = p.TradeType,
                Volume = (decimal)p.Volume,
                OpenPrice = (decimal)p.OpenPrice,
                CurrentPrice = (decimal)p.CurrentPrice,
                StopLoss = (decimal)p.StopLoss,
                TakeProfit = (decimal)p.TakeProfit,
                Profit = (decimal)p.Profit,
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get MT5 positions");
            return Enumerable.Empty<MT5PositionInfo>();
        }
    }

    public async Task<MT5AccountInfo> GetAccountInfoAsync(CancellationToken ct = default)
    {
        var response = await _http.GetFromJsonAsync<MT5BridgeResponse<MT5AccountData>>(
            "/api/v1/mt5/account-info", ct);

        if (response?.Success != true || response.Data == null)
            throw new InvalidOperationException("Failed to get account info from MT5");

        return new MT5AccountInfo
        {
            Balance = (decimal)response.Data.Balance,
            Equity = (decimal)response.Data.Equity,
            Margin = (decimal)response.Data.Margin,
            FreeMargin = (decimal)response.Data.FreeMargin,
            MarginLevel = (decimal)response.Data.MarginLevel,
            Currency = response.Data.Currency,
        };
    }

    public async Task<bool> IsConnectedAsync(CancellationToken ct = default)
    {
        try
        {
            var response = await _http.GetFromJsonAsync<MT5BridgeResponse<object>>(
                "/api/v1/mt5/health", ct);
            return response?.Success == true;
        }
        catch { return false; }
    }

    // ── Private DTOs ─────────────────────────────────────
    private record MT5BridgeResponse<T>(bool Success, T? Data, List<string>? Errors);

    private record MT5TradeData(
        string BrokerTicket,
        string ExecutionStatus,
        double ExecutedPrice,
        double Slippage,
        string? ErrorMessage);

    private record MT5PositionData(
        string Ticket, string Symbol, string TradeType,
        double Volume, double OpenPrice, double CurrentPrice,
        double StopLoss, double TakeProfit, double Profit);

    private record MT5AccountData(
        double Balance, double Equity, double Margin,
        double FreeMargin, double MarginLevel, string Currency);
}