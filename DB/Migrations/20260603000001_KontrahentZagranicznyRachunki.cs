using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProFak.DB.Migrations
{
    /// <inheritdoc />
    public partial class KontrahentZagranicznyRachunki : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DodatkoweRachunki",
                table: "Kontrahent",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "CzyZagraniczny",
                table: "Kontrahent",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "KrajKontrahenta",
                table: "Kontrahent",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ZagranicznyNumerVAT",
                table: "Kontrahent",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DodatkoweRachunki",
                table: "Kontrahent");

            migrationBuilder.DropColumn(
                name: "CzyZagraniczny",
                table: "Kontrahent");

            migrationBuilder.DropColumn(
                name: "KrajKontrahenta",
                table: "Kontrahent");

            migrationBuilder.DropColumn(
                name: "ZagranicznyNumerVAT",
                table: "Kontrahent");
        }
    }
}
