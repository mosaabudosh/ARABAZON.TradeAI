using ARABAZON.TradeAI.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ARABAZON.TradeAI.Persistence.Configurations;

public class IndicatorConfiguration : IEntityTypeConfiguration<Indicator>
{
    public void Configure(EntityTypeBuilder<Indicator> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseIdentityColumn();
        builder.Property(x => x.IndicatorType).HasMaxLength(20).IsRequired();
        builder.Property(x => x.Value).HasPrecision(18, 8);
        builder.HasIndex(x => new { x.CandleId, x.IndicatorType }).IsUnique();
    }
}