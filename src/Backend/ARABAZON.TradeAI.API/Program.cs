using ARABAZON.TradeAI.API.Hubs;
using ARABAZON.TradeAI.Application;
using ARABAZON.TradeAI.Application.Interfaces;
using ARABAZON.TradeAI.Infrastructure;
using ARABAZON.TradeAI.Persistence;
using ARABAZON.TradeAI.Persistence.Context;
using ARABAZON.TradeAI.Workers;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .Enrich.WithEnvironmentName()
    .Enrich.WithThreadId()
    .CreateLogger();

builder.Host.UseSerilog();

// Services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "ARABAZON Trade AI API", Version = "v1" });
});

builder.Services.AddSignalR();
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration); 
builder.Services.AddPersistence(builder.Configuration);

// Register SignalR Hub Notifier
builder.Services.AddScoped<IMarketHubNotifier, MarketHubNotifier>();
// SignalR notifiers
builder.Services.AddScoped<IMarketHubNotifier, MarketHubNotifier>();
builder.Services.AddScoped<ISignalHubNotifier, SignalHubNotifier>();
// Notifiers
builder.Services.AddScoped<IMarketHubNotifier, MarketHubNotifier>();
builder.Services.AddScoped<ISignalHubNotifier, SignalHubNotifier>();
builder.Services.AddScoped<ITradeHubNotifier, TradeHubNotifier>();
// Register Workers
builder.Services.AddHostedService<MarketDataWorker>();
builder.Services.AddHostedService<MarketDataWorker>();
builder.Services.AddHostedService<SignalScannerWorker>();
builder.Services.AddHostedService<RiskMonitorWorker>();
builder.Services.AddHostedService<MarketDataWorker>();
builder.Services.AddHostedService<SignalScannerWorker>();
builder.Services.AddHostedService<RiskMonitorWorker>();
builder.Services.AddHostedService<TradeMonitorWorker>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials());
});

var app = builder.Build();

// Auto migrate on startup
// Seed symbols
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();

    if (!db.Symbols.Any())
    {
        db.Symbols.AddRange(
            ARABAZON.TradeAI.Domain.Entities.Symbol.Create("XAUUSD", "Gold", ARABAZON.TradeAI.Domain.Enums.MarketType.Commodity, 0.01m, 100m),
            ARABAZON.TradeAI.Domain.Entities.Symbol.Create("USOIL", "Crude Oil WTI", ARABAZON.TradeAI.Domain.Enums.MarketType.Commodity, 0.01m, 1000m)
        );
        await db.SaveChangesAsync();
        Log.Information("Symbols seeded.");
    }

    // Seed SymbolRiskConfigurations
    if (!db.SymbolRiskConfigurations.Any())
    {
        var gold = db.Symbols.First(s => s.SymbolCode == "XAUUSD");
        var oil = db.Symbols.First(s => s.SymbolCode == "USOIL");

        db.SymbolRiskConfigurations.AddRange(
            ARABAZON.TradeAI.Domain.Entities.SymbolRiskConfiguration.CreateForGold(gold.Id),
            ARABAZON.TradeAI.Domain.Entities.SymbolRiskConfiguration.CreateForOil(oil.Id)
        );
        await db.SaveChangesAsync();
        Log.Information("SymbolRiskConfigurations seeded.");
    }
}



if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseSerilogRequestLogging();
app.UseCors("AllowAngular");
app.UseAuthorization();
app.MapControllers();

// SignalR Hubs (to be added)
app.MapHub<MarketHub>("/hubs/market");
app.MapHub<SignalHub>("/hubs/signals");
app.MapHub<TradeHub>("/hubs/trade");

Log.Information("ARABAZON Trade AI API started.");
app.Run();