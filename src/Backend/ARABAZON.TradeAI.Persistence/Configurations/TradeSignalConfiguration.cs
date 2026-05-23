using ARABAZON.TradeAI.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ARABAZON.TradeAI.Persistence.Configurations;

public class TradeSignalConfiguration : IEntityTypeConfiguration<TradeSignal>
{
    public void Configure(EntityTypeBuilder<TradeSignal> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.StrategyName).HasMaxLength(100).IsRequired();
        builder.Property(x => x.SignalType).HasConversion<string>();
        builder.Property(x => x.Status).HasConversion<string>();

        builder.OwnsOne(x => x.EntryPrice, p => p.Property(v => v.Value).HasColumnName("EntryPrice").HasPrecision(18, 5));
        builder.OwnsOne(x => x.StopLoss, p => p.Property(v => v.Value).HasColumnName("StopLoss").HasPrecision(18, 5));
        builder.OwnsOne(x => x.TakeProfit, p => p.Property(v => v.Value).HasColumnName("TakeProfit").HasPrecision(18, 5));
        builder.OwnsOne(x => x.ConfidenceScore, p => p.Property(v => v.Value).HasColumnName("ConfidenceScore").HasPrecision(5, 2));
        builder.OwnsOne(x => x.RiskPercentage, p => p.Property(v => v.Value).HasColumnName("RiskPercentage").HasPrecision(5, 2));

        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => x.GeneratedAt);
    }
}