using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjetoLabKarts.Migrations
{
    /// <inheritdoc />
    public partial class f4 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "SelectedProgressBar",
                table: "AppSettings",
                newName: "SelectedProgressBars");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "SelectedProgressBars",
                table: "AppSettings",
                newName: "SelectedProgressBar");
        }
    }
}
