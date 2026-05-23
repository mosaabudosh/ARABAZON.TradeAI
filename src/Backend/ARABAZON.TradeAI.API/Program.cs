using ARABAZON.TradeAI.Application;
using ARABAZON.TradeAI.Infrastructure;
using ARABAZON.TradeAI.Persistence;
using ARABAZON.TradeAI.Persistence.Context;
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
builder.Services.AddInfrastructure();
//builder.Services.AddPersistence(builder.Configuration);

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
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();
    Log.Information("Database migration applied successfully.");
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
// app.MapHub<MarketHub>("/hubs/market");
// app.MapHub<TradeHub>("/hubs/trade");

Log.Information("ARABAZON Trade AI API started.");
app.Run();