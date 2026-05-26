using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ARABAZON.TradeAI.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddMarketSnapshot : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MarketSnapshots",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SymbolId = table.Column<Guid>(type: "uuid", nullable: false),
                    BidPrice = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    AskPrice = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    Spread = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    Volatility = table.Column<string>(type: "text", nullable: false),
                    SnapshotTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MarketSnapshots", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MarketSnapshots_SymbolId_SnapshotTime",
                table: "MarketSnapshots",
                columns: new[] { "SymbolId", "SnapshotTime" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MarketSnapshots");
        }
    }
}
