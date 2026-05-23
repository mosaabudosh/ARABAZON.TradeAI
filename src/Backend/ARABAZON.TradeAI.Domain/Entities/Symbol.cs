using ARABAZON.TradeAI.Domain.Common;
using ARABAZON.TradeAI.Domain.Enums;

namespace ARABAZON.TradeAI.Domain.Entities;

public class Symbol : BaseEntity
{
    public string SymbolCode { get; private set; } = default!;
    public string Name { get; private set; } = default!;
    public MarketType MarketType { get; private set; }
    public decimal TickSize { get; private set; }
    public decimal ContractSize { get; private set; }
    public bool IsTradable { get; private set; }
    public bool IsActive { get; private set; }

    private Symbol() { }

    public static Symbol Create(string symbolCode, string name, MarketType marketType,
        decimal tickSize, decimal contractSize)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(symbolCode);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        return new Symbol
        {
            SymbolCode = symbolCode.ToUpperInvariant(),
            Name = name,
            MarketType = marketType,
            TickSize = tickSize,
            ContractSize = contractSize,
            IsTradable = true,
            IsActive = true
        };
    }

    public void Deactivate() { IsActive = false; SetUpdatedAt(); }
    public void Activate() { IsActive = true; SetUpdatedAt(); }
}