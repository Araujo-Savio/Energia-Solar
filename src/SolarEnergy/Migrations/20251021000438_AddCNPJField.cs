using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SolarEnergy.Migrations
{
    /// <inheritdoc />
    public partial class AddCNPJField : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CNPJ",
                table: "AspNetUsers",
                type: "nvarchar(18)",
                maxLength: 18,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_CNPJ",
                table: "AspNetUsers",
                column: "CNPJ",
                unique: true,
                filter: "[CNPJ] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_CNPJ",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "CNPJ",
                table: "AspNetUsers");
        }
    }
}
