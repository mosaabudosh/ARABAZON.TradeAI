using ARABAZON.TradeAI.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ARABAZON.TradeAI.Persistence.Configurations;

public class TradeConfiguration : IEntityTypeConfiguration<Trade>
{
    public void Configure(EntityTypeBuilder<Trade> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.BrokerTicket).HasMaxLength(50);
        builder.Property(x => x.TradeType).HasConversion<string>();
        builder.Property(x => x.Status).HasConversion<string>();

        builder.OwnsOne(x => x.EntryPrice, p => p.Property(v => v.Value).HasColumnName("EntryPrice").HasPrecision(18, 5));
        builder.OwnsOne(x => x.ExitPrice, p => p.Property(v => v.Value).HasColumnName("ExitPrice").HasPrecision(18, 5));
        builder.OwnsOne(x => x.StopLoss, p => p.Property(v => v.Value).HasColumnName("StopLoss").HasPrecision(18, 5));
        builder.OwnsOne(x => x.TakeProfit, p => p.Property(v => v.Value).HasColumnName("TakeProfit").HasPrecision(18, 5));
        builder.OwnsOne(x => x.ProfitLoss, p =>
        {
            p.Property(v => v.Amount).HasColumnName("ProfitLossAmount").HasPrecision(18, 2);
            p.Property(v => v.Currency).HasColumnName("ProfitLossCurrency").HasMaxLength(3);
        });

        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => x.OpenedAt);
    }
}