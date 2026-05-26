using System.Net.Http.Json;
using ARABAZON.TradeAI.Application.Interfaces;
using ARABAZON.TradeAI.Domain.Entities;
using ARABAZON.TradeAI.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace ARABAZON.TradeAI.Infrastructure.MarketData;

public class MT5MarketDataAdapter : IMarketDataService
{
    private readonly HttpClient _http;
    private readonly ILogger<MT5MarketDataAdapter> _logger;

    public MT5MarketDataAdapter(HttpClient http, ILogger<MT5MarketDataAdapter> logger)
    {
        _http = http;
        _logger = logger;
    }

    public async Task<IEnumerable<Candle>> GetCandlesAsync(
        string symbol, Timeframe timeframe, int count = 100, CancellationToken ct = default)
    {
        try
        {
            var url = $"/api/v1/mt5/candles?symbol={symbol}&timeframe={timeframe}&count={count}";
            var response = await _http.GetFromJsonAsync<MT5Response<List<CandleData>>>(url, ct);

            if (response?.Success != true || response.Data == null)
                return Enumerable.Empty<Candle>();

            // We need symbolId — resolve from symbol code
            // For now return raw data; worker will attach SymbolId
            return response.Data.Select(c => Candle.Create(
                Guid.Empty, // placeholder — worker sets the real SymbolId
                timeframe,
                (decimal)c.OpenPrice,
                (decimal)c.HighPrice,
                (decimal)c.LowPrice,
                (decimal)c.ClosePrice,
                (decimal)c.Volume,
                c.OpenTime,
                c.CloseTime
            ));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch candles for {Symbol} {Timeframe}", symbol, timeframe);
            return Enumerable.Empty<Candle>();
        }
    }

    public async Task<MarketSnapshot> GetLiveSnapshotAsync(string symbol, CancellationToken ct = default)
    {
        var url = $"/api/v1/mt5/tick?symbol={symbol}";
        var response = await _http.GetFromJsonAsync<MT5Response<TickData>>(url, ct);

        if (response?.Success != true || response.Data == null)
            throw new InvalidOperationException($"Failed to get tick for {symbol}");

        return MarketSnapshot.Create(
            Guid.Empty, // placeholder
            (decimal)response.Data.Bid,
            (decimal)response.Data.Ask
        );
    }

    public async Task<bool> IsConnectedAsync(CancellationToken ct = default)
    {
        try
        {
            var response = await _http.GetFromJsonAsync<MT5Response<object>>("/api/v1/mt5/health", ct);
            return response?.Success == true;
        }
        catch
        {
            return false;
        }
    }

    // ── DTOs ──────────────────────────────────────────────
    private record MT5Response<T>(bool Success, T? Data, List<string> Errors);
    private record CandleData(
        double OpenPrice, double HighPrice, double LowPrice, double ClosePrice,
        double Volume, DateTime OpenTime, DateTime CloseTime);
    private record TickData(double Bid, double Ask, double Spread, DateTime Timestamp);
}