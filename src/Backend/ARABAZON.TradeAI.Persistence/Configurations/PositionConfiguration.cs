using ARABAZON.TradeAI.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ARABAZON.TradeAI.Persistence.Configurations;

public class PositionConfiguration : IEntityTypeConfiguration<Position>
{
    public void Configure(EntityTypeBuilder<Position> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Exposure).HasPrecision(18, 4);

        builder.OwnsOne(x => x.CurrentPrice, owned =>
            owned.Property(v => v.Value).HasColumnName("CurrentPrice").HasPrecision(18, 5).IsRequired());

        builder.OwnsOne(x => x.UnrealizedPnL, owned =>
        {
            owned.Property(v => v.Amount).HasColumnName("UnrealizedPnLAmount").HasPrecision(18, 2);
            owned.Property(v => v.Currency).HasColumnName("UnrealizedPnLCurrency").HasMaxLength(3);
        });

        builder.HasIndex(x => x.TradeId).IsUnique();
    }
}