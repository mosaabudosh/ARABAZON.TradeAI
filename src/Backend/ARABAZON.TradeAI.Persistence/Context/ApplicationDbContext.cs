using ARABAZON.TradeAI.Application.Interfaces;
using ARABAZON.TradeAI.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace ARABAZON.TradeAI.Persistence.Context;

public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<Symbol> Symbols => Set<Symbol>();
    public DbSet<Candle> Candles => Set<Candle>();
    public DbSet<TradeSignal> TradeSignals => Set<TradeSignal>();
    public DbSet<Trade> Trades => Set<Trade>();
    public DbSet<RiskConfiguration> RiskConfigurations => Set<RiskConfiguration>();
    public DbSet<MarketSnapshot> MarketSnapshots => Set<MarketSnapshot>();
    public DbSet<Indicator> Indicators => Set<Indicator>();
    public DbSet<RiskEvent> RiskEvents => Set<RiskEvent>();
    public DbSet<SymbolRiskConfiguration> SymbolRiskConfigurations => Set<SymbolRiskConfiguration>();
    public DbSet<TradeExecution> TradeExecutions => Set<TradeExecution>();
    public DbSet<Position> Positions => Set<Position>();
    public DbSet<ExecutionAuditLog> ExecutionAuditLogs => Set<ExecutionAuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}