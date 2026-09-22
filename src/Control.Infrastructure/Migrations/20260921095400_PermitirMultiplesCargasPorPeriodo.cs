using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Control.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class PermitirMultiplesCargasPorPeriodo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CargasArchivo_Periodo",
                table: "CargasArchivo");

            migrationBuilder.CreateIndex(
                name: "IX_CargasArchivo_Periodo",
                table: "CargasArchivo",
                column: "Periodo");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CargasArchivo_Periodo",
                table: "CargasArchivo");

            migrationBuilder.CreateIndex(
                name: "IX_CargasArchivo_Periodo",
                table: "CargasArchivo",
                column: "Periodo",
                unique: true);
        }
    }
}
