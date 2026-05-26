namespace ARABAZON.TradeAI.Domain.Indicators;

public static class EmaCalculator
{
    public static decimal[] Calculate(decimal[] prices, int period)
    {
        if (prices.Length < period)
            return Array.Empty<decimal>();

        var ema = new decimal[prices.Length];
        decimal multiplier = 2m / (period + 1);

        // Seed with SMA for first period
        decimal sum = 0;
        for (int i = 0; i < period; i++)
            sum += prices[i];

        ema[period - 1] = sum / period;

        for (int i = period; i < prices.Length; i++)
            ema[i] = (prices[i] - ema[i - 1]) * multiplier + ema[i - 1];

        return ema;
    }

    public static decimal? Latest(decimal[] prices, int period)
    {
        var result = Calculate(prices, period);
        return result.Length > 0 ? result[^1] : null;
    }
}