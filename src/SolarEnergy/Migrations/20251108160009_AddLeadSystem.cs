using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SolarEnergy.Migrations
{
    /// <inheritdoc />
    public partial class AddLeadSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CompanyLeadBalances",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    AvailableLeads = table.Column<int>(type: "int", nullable: false),
                    ConsumedLeads = table.Column<int>(type: "int", nullable: false),
                    TotalPurchasedLeads = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompanyLeadBalances", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CompanyLeadBalances_AspNetUsers_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LeadConsumptions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    QuoteId = table.Column<int>(type: "int", nullable: false),
                    ConsumedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    CompanyLeadBalanceId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LeadConsumptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LeadConsumptions_AspNetUsers_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LeadConsumptions_CompanyLeadBalances_CompanyLeadBalanceId",
                        column: x => x.CompanyLeadBalanceId,
                        principalTable: "CompanyLeadBalances",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_LeadConsumptions_Quotes_QuoteId",
                        column: x => x.QuoteId,
                        principalTable: "Quotes",
                        principalColumn: "QuoteId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LeadPurchases",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    LeadQuantity = table.Column<int>(type: "int", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    DiscountPercentage = table.Column<decimal>(type: "decimal(18,2)", precision: 5, scale: 2, nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    PurchaseDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PaymentStatus = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TransactionId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CompanyLeadBalanceId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LeadPurchases", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LeadPurchases_AspNetUsers_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LeadPurchases_CompanyLeadBalances_CompanyLeadBalanceId",
                        column: x => x.CompanyLeadBalanceId,
                        principalTable: "CompanyLeadBalances",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_CompanyLeadBalances_CompanyId",
                table: "CompanyLeadBalances",
                column: "CompanyId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LeadConsumptions_CompanyId",
                table: "LeadConsumptions",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_LeadConsumptions_CompanyId_QuoteId",
                table: "LeadConsumptions",
                columns: new[] { "CompanyId", "QuoteId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LeadConsumptions_CompanyLeadBalanceId",
                table: "LeadConsumptions",
                column: "CompanyLeadBalanceId");

            migrationBuilder.CreateIndex(
                name: "IX_LeadConsumptions_ConsumedAt",
                table: "LeadConsumptions",
                column: "ConsumedAt");

            migrationBuilder.CreateIndex(
                name: "IX_LeadConsumptions_QuoteId",
                table: "LeadConsumptions",
                column: "QuoteId");

            migrationBuilder.CreateIndex(
                name: "IX_LeadPurchases_CompanyId",
                table: "LeadPurchases",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_LeadPurchases_CompanyLeadBalanceId",
                table: "LeadPurchases",
                column: "CompanyLeadBalanceId");

            migrationBuilder.CreateIndex(
                name: "IX_LeadPurchases_PurchaseDate",
                table: "LeadPurchases",
                column: "PurchaseDate");

            migrationBuilder.CreateIndex(
                name: "IX_LeadPurchases_TransactionId",
                table: "LeadPurchases",
                column: "TransactionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LeadConsumptions");

            migrationBuilder.DropTable(
                name: "LeadPurchases");

            migrationBuilder.DropTable(
                name: "CompanyLeadBalances");
        }
    }
}
