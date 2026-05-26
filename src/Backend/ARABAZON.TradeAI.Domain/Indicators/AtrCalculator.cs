namespace ARABAZON.TradeAI.Domain.Indicators;

public static class AtrCalculator
{
    public static decimal[] Calculate(decimal[] highs, decimal[] lows, decimal[] closes, int period = 14)
    {
        if (highs.Length < period + 1)
            return Array.Empty<decimal>();

        var atr = new decimal[highs.Length];

        // True Range for each candle
        var tr = new decimal[highs.Length];
        tr[0] = highs[0] - lows[0];

        for (int i = 1; i < highs.Length; i++)
        {
            decimal hl = highs[i] - lows[i];
            decimal hc = Math.Abs(highs[i] - closes[i - 1]);
            decimal lc = Math.Abs(lows[i] - closes[i - 1]);
            tr[i] = Math.Max(hl, Math.Max(hc, lc));
        }

        // Initial ATR = simple average
        decimal sum = 0;
        for (int i = 0; i < period; i++)
            sum += tr[i];
        atr[period - 1] = sum / period;

        // Smoothed ATR (Wilder)
        for (int i = period; i < highs.Length; i++)
            atr[i] = (atr[i - 1] * (period - 1) + tr[i]) / period;

        return atr;
    }

    public static decimal? Latest(decimal[] highs, decimal[] lows, decimal[] closes, int period = 14)
    {
        var result = Calculate(highs, lows, closes, period);
        return result.Length > 0 ? result[^1] : null;
    }
}