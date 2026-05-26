using ARABAZON.TradeAI.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ARABAZON.TradeAI.Persistence.Configurations;

public class SymbolRiskConfigurationConfig : IEntityTypeConfiguration<SymbolRiskConfiguration>
{
    public void Configure(EntityTypeBuilder<SymbolRiskConfiguration> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.MaxRiskPerTrade).HasPrecision(5, 2);
        builder.Property(x => x.MaxDailyLoss).HasPrecision(5, 2);
        builder.Property(x => x.MaxPositionSize).HasPrecision(18, 4);
        builder.Property(x => x.MaxSpreadAllowed).HasPrecision(10, 3);
        builder.Property(x => x.MaxSlippageAllowed).HasPrecision(10, 5);
        builder.Property(x => x.AtrMultiplierSL).HasPrecision(5, 2);
        builder.Property(x => x.AtrMultiplierTP).HasPrecision(5, 2);
        builder.HasIndex(x => x.SymbolId).IsUnique();
    }
}