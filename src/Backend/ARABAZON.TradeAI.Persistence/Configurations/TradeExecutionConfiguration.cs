using ARABAZON.TradeAI.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ARABAZON.TradeAI.Persistence.Configurations;

public class TradeExecutionConfiguration : IEntityTypeConfiguration<TradeExecution>
{
    public void Configure(EntityTypeBuilder<TradeExecution> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.ExecutionType).HasMaxLength(20).IsRequired();
        builder.Property(x => x.Status).HasConversion<string>();
        builder.Property(x => x.RequestId).HasMaxLength(50).IsRequired();
        builder.Property(x => x.CorrelationId).HasMaxLength(50).IsRequired();
        builder.Property(x => x.BrokerTicket).HasMaxLength(50);
        builder.Property(x => x.ErrorMessage).HasMaxLength(500);
        builder.Property(x => x.Slippage).HasPrecision(18, 5);

        builder.OwnsOne(x => x.RequestedPrice, owned =>
            owned.Property(v => v.Value).HasColumnName("RequestedPrice").HasPrecision(18, 5).IsRequired());

        builder.OwnsOne(x => x.ExecutedPrice, owned =>
            owned.Property(v => v.Value).HasColumnName("ExecutedPrice").HasPrecision(18, 5));

        builder.HasIndex(x => x.TradeId);
        builder.HasIndex(x => x.RequestId);
    }
}