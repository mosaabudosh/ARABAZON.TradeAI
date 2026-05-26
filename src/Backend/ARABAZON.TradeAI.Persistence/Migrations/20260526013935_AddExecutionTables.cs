using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ARABAZON.TradeAI.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddExecutionTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ExecutionAuditLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RequestId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CorrelationId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    TradeId = table.Column<Guid>(type: "uuid", nullable: true),
                    BrokerTicket = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ExecutionAction = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ExecutionStatus = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    RequestedPrice = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: true),
                    ExecutedPrice = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: true),
                    Slippage = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: true),
                    ErrorMessage = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExecutionAuditLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Positions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TradeId = table.Column<Guid>(type: "uuid", nullable: false),
                    CurrentPrice = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    UnrealizedPnLAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    UnrealizedPnLCurrency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    Exposure = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    LastUpdated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Positions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TradeExecutions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TradeId = table.Column<Guid>(type: "uuid", nullable: false),
                    ExecutionType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    RequestedPrice = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    ExecutedPrice = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: true),
                    Slippage = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    ErrorMessage = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    BrokerTicket = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    RequestId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CorrelationId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ExecutedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TradeExecutions", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ExecutionAuditLogs_CorrelationId",
                table: "ExecutionAuditLogs",
                column: "CorrelationId");

            migrationBuilder.CreateIndex(
                name: "IX_ExecutionAuditLogs_CreatedAt",
                table: "ExecutionAuditLogs",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_ExecutionAuditLogs_RequestId",
                table: "ExecutionAuditLogs",
                column: "RequestId");

            migrationBuilder.CreateIndex(
                name: "IX_ExecutionAuditLogs_TradeId",
                table: "ExecutionAuditLogs",
                column: "TradeId");

            migrationBuilder.CreateIndex(
                name: "IX_Positions_TradeId",
                table: "Positions",
                column: "TradeId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TradeExecutions_RequestId",
                table: "TradeExecutions",
                column: "RequestId");

            migrationBuilder.CreateIndex(
                name: "IX_TradeExecutions_TradeId",
                table: "TradeExecutions",
                column: "TradeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ExecutionAuditLogs");

            migrationBuilder.DropTable(
                name: "Positions");

            migrationBuilder.DropTable(
                name: "TradeExecutions");
        }
    }
}
