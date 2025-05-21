using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjetoLabKarts.Migrations
{
    /// <inheritdoc />
    public partial class f0 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ViewName = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RowsPerPage = table.Column<int>(type: "int", nullable: false),
                    SelectedColumns = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppSettings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SessoesKart",
                columns: table => new
                {
                    NomeFicheiro = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    NomePiloto = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NomePista = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NumeroVoltas = table.Column<int>(type: "int", nullable: false),
                    MelhorVolta = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DataHora = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CoordenadaLat = table.Column<double>(type: "float", nullable: false),
                    CoordenadaLong = table.Column<double>(type: "float", nullable: false),
                    VelocidadeMax = table.Column<double>(type: "float", nullable: false),
                    DataHoraInsercao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NumeroMelhorVolta = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SessoesKart", x => x.NomeFicheiro);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppSettings_ViewName",
                table: "AppSettings",
                column: "ViewName",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppSettings");

            migrationBuilder.DropTable(
                name: "SessoesKart");
        }
    }
}
