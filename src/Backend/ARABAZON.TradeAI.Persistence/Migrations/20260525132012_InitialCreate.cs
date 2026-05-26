using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ARABAZON.TradeAI.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Candles",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SymbolId = table.Column<Guid>(type: "uuid", nullable: false),
                    Timeframe = table.Column<string>(type: "text", nullable: false),
                    OpenPrice = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    HighPrice = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    LowPrice = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    ClosePrice = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    Volume = table.Column<decimal>(type: "numeric", nullable: false),
                    OpenTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CloseTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Candles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RiskConfigurations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MaxRiskPerTrade = table.Column<decimal>(type: "numeric", nullable: false),
                    DailyLossLimit = table.Column<decimal>(type: "numeric", nullable: false),
                    MaxConcurrentTrades = table.Column<int>(type: "integer", nullable: false),
                    MaxExposure = table.Column<decimal>(type: "numeric", nullable: false),
                    AutoTradingEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RiskConfigurations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Symbols",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SymbolCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    MarketType = table.Column<string>(type: "text", nullable: false),
                    TickSize = table.Column<decimal>(type: "numeric", nullable: false),
                    ContractSize = table.Column<decimal>(type: "numeric", nullable: false),
                    IsTradable = table.Column<bool>(type: "boolean", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Symbols", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Trades",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SignalId = table.Column<Guid>(type: "uuid", nullable: false),
                    BrokerTicket = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    SymbolId = table.Column<Guid>(type: "uuid", nullable: false),
                    TradeType = table.Column<string>(type: "text", nullable: false),
                    EntryPrice = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    ExitPrice = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: true),
                    StopLoss = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    TakeProfit = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    PositionSize = table.Column<decimal>(type: "numeric", nullable: false),
                    ProfitLossAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    ProfitLossCurrency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    OpenedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ClosedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Trades", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TradeSignals",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SymbolId = table.Column<Guid>(type: "uuid", nullable: false),
                    SignalType = table.Column<string>(type: "text", nullable: false),
                    StrategyName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    EntryPrice = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    StopLoss = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    TakeProfit = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    ConfidenceScore = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    RiskPercentage = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    GeneratedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TradeSignals", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Candles_OpenTime",
                table: "Candles",
                column: "OpenTime");

            migrationBuilder.CreateIndex(
                name: "IX_Candles_SymbolId_Timeframe_OpenTime",
                table: "Candles",
                columns: new[] { "SymbolId", "Timeframe", "OpenTime" });

            migrationBuilder.CreateIndex(
                name: "IX_Symbols_SymbolCode",
                table: "Symbols",
                column: "SymbolCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Trades_OpenedAt",
                table: "Trades",
                column: "OpenedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Trades_Status",
                table: "Trades",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_TradeSignals_GeneratedAt",
                table: "TradeSignals",
                column: "GeneratedAt");

            migrationBuilder.CreateIndex(
                name: "IX_TradeSignals_Status",
                table: "TradeSignals",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Candles");

            migrationBuilder.DropTable(
                name: "RiskConfigurations");

            migrationBuilder.DropTable(
                name: "Symbols");

            migrationBuilder.DropTable(
                name: "Trades");

            migrationBuilder.DropTable(
                name: "TradeSignals");
        }
    }
}
