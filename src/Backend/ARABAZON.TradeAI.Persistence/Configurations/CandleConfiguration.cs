using ARABAZON.TradeAI.Domain.Entities;
using ARABAZON.TradeAI.Domain.Enums;
using ARABAZON.TradeAI.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ARABAZON.TradeAI.Persistence.Configurations;

public class CandleConfiguration : IEntityTypeConfiguration<Candle>
{
    public void Configure(EntityTypeBuilder<Candle> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseIdentityColumn();
        builder.Property(x => x.Timeframe).HasConversion<string>();

        builder.OwnsOne(x => x.OpenPrice, p => p.Property(v => v.Value).HasColumnName("OpenPrice").HasPrecision(18, 5));
        builder.OwnsOne(x => x.HighPrice, p => p.Property(v => v.Value).HasColumnName("HighPrice").HasPrecision(18, 5));
        builder.OwnsOne(x => x.LowPrice, p => p.Property(v => v.Value).HasColumnName("LowPrice").HasPrecision(18, 5));
        builder.OwnsOne(x => x.ClosePrice, p => p.Property(v => v.Value).HasColumnName("ClosePrice").HasPrecision(18, 5));

        builder.HasIndex(x => new { x.SymbolId, x.Timeframe, x.OpenTime });
        builder.HasIndex(x => x.OpenTime);
    }
}