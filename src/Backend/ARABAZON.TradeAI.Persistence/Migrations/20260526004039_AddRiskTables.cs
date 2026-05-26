using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ARABAZON.TradeAI.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddRiskTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RiskEvents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EventType = table.Column<string>(type: "text", nullable: false),
                    Severity = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    SymbolId = table.Column<Guid>(type: "uuid", nullable: true),
                    CurrentValue = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: true),
                    ThresholdValue = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: true),
                    TriggeredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RiskEvents", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SymbolRiskConfigurations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SymbolId = table.Column<Guid>(type: "uuid", nullable: false),
                    MaxRiskPerTrade = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    MaxDailyLoss = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    MaxPositionSize = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    MaxSpreadAllowed = table.Column<decimal>(type: "numeric(10,3)", precision: 10, scale: 3, nullable: false),
                    MaxSlippageAllowed = table.Column<decimal>(type: "numeric(10,5)", precision: 10, scale: 5, nullable: false),
                    MaxConcurrentTrades = table.Column<int>(type: "integer", nullable: false),
                    AtrMultiplierSL = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    AtrMultiplierTP = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    AutoTradingEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SymbolRiskConfigurations", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RiskEvents_EventType",
                table: "RiskEvents",
                column: "EventType");

            migrationBuilder.CreateIndex(
                name: "IX_RiskEvents_TriggeredAt",
                table: "RiskEvents",
                column: "TriggeredAt");

            migrationBuilder.CreateIndex(
                name: "IX_SymbolRiskConfigurations_SymbolId",
                table: "SymbolRiskConfigurations",
                column: "SymbolId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RiskEvents");

            migrationBuilder.DropTable(
                name: "SymbolRiskConfigurations");
        }
    }
}
