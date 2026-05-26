using ARABAZON.TradeAI.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ARABAZON.TradeAI.Persistence.Configurations;

public class ExecutionAuditLogConfiguration : IEntityTypeConfiguration<ExecutionAuditLog>
{
    public void Configure(EntityTypeBuilder<ExecutionAuditLog> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.RequestId).HasMaxLength(50).IsRequired();
        builder.Property(x => x.CorrelationId).HasMaxLength(50).IsRequired();
        builder.Property(x => x.BrokerTicket).HasMaxLength(50);
        builder.Property(x => x.ExecutionAction).HasMaxLength(20).IsRequired();
        builder.Property(x => x.ExecutionStatus).HasMaxLength(20).IsRequired();
        builder.Property(x => x.ErrorMessage).HasMaxLength(500);
        builder.Property(x => x.RequestedPrice).HasPrecision(18, 5);
        builder.Property(x => x.ExecutedPrice).HasPrecision(18, 5);
        builder.Property(x => x.Slippage).HasPrecision(18, 5);

        builder.HasIndex(x => x.RequestId);
        builder.HasIndex(x => x.CorrelationId);
        builder.HasIndex(x => x.TradeId);
        builder.HasIndex(x => x.CreatedAt);
    }
}