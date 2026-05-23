namespace ARABAZON.TradeAI.Domain.ValueObjects;

public sealed record RiskRatio
{
    public decimal Risk { get; }
    public decimal Reward { get; }
    public decimal Ratio => Risk == 0 ? 0 : Math.Round(Reward / Risk, 2);

    public RiskRatio(decimal risk, decimal reward)
    {
        if (risk <= 0) throw new ArgumentException("Risk must be positive.", nameof(risk));
        if (reward <= 0) throw new ArgumentException("Reward must be positive.", nameof(reward));
        Risk = risk;
        Reward = reward;
    }

    public override string ToString() => $"1:{Ratio}";
}