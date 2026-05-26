using ARABAZON.TradeAI.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace ARABAZON.TradeAI.Application.Interfaces;

public interface IApplicationDbContext
{
    DbSet<Symbol> Symbols { get; }
    DbSet<Candle> Candles { get; }
    DbSet<TradeSignal> TradeSignals { get; }
    DbSet<Trade> Trades { get; }
    DbSet<RiskConfiguration> RiskConfigurations { get; }
    DbSet<MarketSnapshot> MarketSnapshots { get; }
    DbSet<Indicator> Indicators { get; }
    DbSet<RiskEvent> RiskEvents { get; }
    DbSet<SymbolRiskConfiguration> SymbolRiskConfigurations { get; }
    DbSet<TradeExecution> TradeExecutions { get; }
    DbSet<Position> Positions { get; }
    DbSet<ExecutionAuditLog> ExecutionAuditLogs { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}