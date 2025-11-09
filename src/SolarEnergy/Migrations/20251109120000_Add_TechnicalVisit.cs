using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SolarEnergy.Migrations
{
    /// <inheritdoc />
    public partial class Add_TechnicalVisit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TechnicalVisits",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    ClientId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    ServiceType = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    VisitDate = table.Column<DateTime>(type: "date", nullable: false),
                    VisitTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    Address = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TechnicalVisits", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TechnicalVisits_AspNetUsers_ClientId",
                        column: x => x.ClientId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TechnicalVisits_AspNetUsers_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TechnicalVisits_ClientId",
                table: "TechnicalVisits",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_TechnicalVisits_CompanyId",
                table: "TechnicalVisits",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_TechnicalVisits_Status",
                table: "TechnicalVisits",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_TechnicalVisits_VisitDate",
                table: "TechnicalVisits",
                column: "VisitDate");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TechnicalVisits");
        }
    }
}
