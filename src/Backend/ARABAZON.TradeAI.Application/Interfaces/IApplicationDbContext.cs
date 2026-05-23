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

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}