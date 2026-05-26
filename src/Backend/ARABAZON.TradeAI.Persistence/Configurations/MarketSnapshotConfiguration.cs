using ARABAZON.TradeAI.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ARABAZON.TradeAI.Persistence.Configurations;

public class MarketSnapshotConfiguration : IEntityTypeConfiguration<MarketSnapshot>
{
    public void Configure(EntityTypeBuilder<MarketSnapshot> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.BidPrice).HasPrecision(18, 5);
        builder.Property(x => x.AskPrice).HasPrecision(18, 5);
        builder.Property(x => x.Spread).HasPrecision(18, 5);
        builder.Property(x => x.Volatility).HasConversion<string>();
        builder.HasIndex(x => new { x.SymbolId, x.SnapshotTime });
    }
}