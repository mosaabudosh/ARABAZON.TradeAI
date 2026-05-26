using ARABAZON.TradeAI.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ARABAZON.TradeAI.Persistence.Configurations;

public class RiskEventConfiguration : IEntityTypeConfiguration<RiskEvent>
{
    public void Configure(EntityTypeBuilder<RiskEvent> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.EventType).HasConversion<string>();
        builder.Property(x => x.Severity).HasMaxLength(20).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(500).IsRequired();
        builder.Property(x => x.CurrentValue).HasPrecision(18, 5);
        builder.Property(x => x.ThresholdValue).HasPrecision(18, 5);
        builder.HasIndex(x => x.TriggeredAt);
        builder.HasIndex(x => x.EventType);
    }
}