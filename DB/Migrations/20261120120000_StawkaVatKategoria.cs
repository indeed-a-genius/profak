using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProFak.DB.Migrations
{
    /// <inheritdoc />
    public partial class StawkaVatKategoria : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Kategoria",
                table: "StawkaVat",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "NettoEksport",
                table: "DeklaracjaVat",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "NettoOdwrotneObciazenie",
                table: "DeklaracjaVat",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "NettoPozaKrajem",
                table: "DeklaracjaVat",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "NettoUslugiUE",
                table: "DeklaracjaVat",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m);

            // Rozpoznanie kategorii istniejących stawek 0% na podstawie ich skrótu.
            // Wartości odpowiadają kolejnym pozycjom enum KategoriaStawkiVat.
            migrationBuilder.Sql("UPDATE StawkaVat SET Kategoria = 1 WHERE Wartosc = 0 AND lower(Skrot) LIKE '%zw%'");
            migrationBuilder.Sql("UPDATE StawkaVat SET Kategoria = 2 WHERE Wartosc = 0 AND Kategoria = 0 AND lower(Skrot) LIKE '%wdt%'");
            migrationBuilder.Sql("UPDATE StawkaVat SET Kategoria = 3 WHERE Wartosc = 0 AND Kategoria = 0 AND lower(Skrot) LIKE '%ex%'");
            migrationBuilder.Sql("UPDATE StawkaVat SET Kategoria = 6 WHERE Wartosc = 0 AND Kategoria = 0 AND lower(Skrot) LIKE '%oo%'");
            migrationBuilder.Sql("UPDATE StawkaVat SET Kategoria = 5 WHERE Wartosc = 0 AND Kategoria = 0 AND (lower(Skrot) LIKE '%np ii%' OR lower(Skrot) LIKE '%npii%' OR lower(Skrot) LIKE '%np.ii%')");
            migrationBuilder.Sql("UPDATE StawkaVat SET Kategoria = 4 WHERE Wartosc = 0 AND Kategoria = 0 AND (lower(Skrot) LIKE '%np i%' OR lower(Skrot) LIKE '%npi%' OR lower(Skrot) LIKE '%np.i%')");
            // Stawki "NP" bez oznaczenia wariantu trafiają do np II (usługi z art. 100 ust. 1 pkt 4)
            migrationBuilder.Sql("UPDATE StawkaVat SET Kategoria = 5 WHERE Wartosc = 0 AND Kategoria = 0 AND lower(Skrot) LIKE '%np%'");

            // Uzupełnienie istniejącego słownika o kategorie, których nie da się wyrazić dotychczasowymi stawkami.
            // Warunek EXISTS pomija nowo zakładane bazy - te wypełnia DaneStartowe.Zaladuj.
            migrationBuilder.Sql("INSERT INTO StawkaVat (Skrot, Wartosc, CzyDomyslna, Kategoria) SELECT '0% EX', 0, 0, 3 WHERE EXISTS (SELECT 1 FROM StawkaVat) AND NOT EXISTS (SELECT 1 FROM StawkaVat WHERE Kategoria = 3)");
            migrationBuilder.Sql("INSERT INTO StawkaVat (Skrot, Wartosc, CzyDomyslna, Kategoria) SELECT 'NP I', 0, 0, 4 WHERE EXISTS (SELECT 1 FROM StawkaVat) AND NOT EXISTS (SELECT 1 FROM StawkaVat WHERE Kategoria = 4)");
            migrationBuilder.Sql("INSERT INTO StawkaVat (Skrot, Wartosc, CzyDomyslna, Kategoria) SELECT 'NP II', 0, 0, 5 WHERE EXISTS (SELECT 1 FROM StawkaVat) AND NOT EXISTS (SELECT 1 FROM StawkaVat WHERE Kategoria = 5)");
            migrationBuilder.Sql("INSERT INTO StawkaVat (Skrot, Wartosc, CzyDomyslna, Kategoria) SELECT 'OO', 0, 0, 6 WHERE EXISTS (SELECT 1 FROM StawkaVat) AND NOT EXISTS (SELECT 1 FROM StawkaVat WHERE Kategoria = 6)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Kategoria",
                table: "StawkaVat");

            migrationBuilder.DropColumn(
                name: "NettoEksport",
                table: "DeklaracjaVat");

            migrationBuilder.DropColumn(
                name: "NettoOdwrotneObciazenie",
                table: "DeklaracjaVat");

            migrationBuilder.DropColumn(
                name: "NettoPozaKrajem",
                table: "DeklaracjaVat");

            migrationBuilder.DropColumn(
                name: "NettoUslugiUE",
                table: "DeklaracjaVat");
        }
    }
}
