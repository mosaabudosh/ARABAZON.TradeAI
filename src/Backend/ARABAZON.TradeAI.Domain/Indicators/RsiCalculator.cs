namespace ARABAZON.TradeAI.Domain.Indicators;

public static class RsiCalculator
{
    public static decimal[] Calculate(decimal[] prices, int period = 14)
    {
        if (prices.Length < period + 1)
            return Array.Empty<decimal>();

        var rsi = new decimal[prices.Length];
        decimal avgGain = 0, avgLoss = 0;

        // Initial average gain/loss
        for (int i = 1; i <= period; i++)
        {
            decimal change = prices[i] - prices[i - 1];
            if (change > 0) avgGain += change;
            else avgLoss += Math.Abs(change);
        }

        avgGain /= period;
        avgLoss /= period;

        rsi[period] = avgLoss == 0 ? 100 : 100 - (100 / (1 + avgGain / avgLoss));

        for (int i = period + 1; i < prices.Length; i++)
        {
            decimal change = prices[i] - prices[i - 1];
            decimal gain = change > 0 ? change : 0;
            decimal loss = change < 0 ? Math.Abs(change) : 0;

            avgGain = (avgGain * (period - 1) + gain) / period;
            avgLoss = (avgLoss * (period - 1) + loss) / period;

            rsi[i] = avgLoss == 0 ? 100 : 100 - (100 / (1 + avgGain / avgLoss));
        }

        return rsi;
    }

    public static decimal? Latest(decimal[] prices, int period = 14)
    {
        var result = Calculate(prices, period);
        return result.Length > 0 ? result[^1] : null;
    }
}