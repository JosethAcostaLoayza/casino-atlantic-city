using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CargaMasiva.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DataProcesada",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    IdCarga = table.Column<Guid>(type: "uuid", nullable: false),
                    Periodo = table.Column<string>(type: "character varying(6)", maxLength: 6, nullable: false),
                    CodigoProducto = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Descripcion = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    Precio = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    FechaRegistro = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DataProcesada", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DetalleCarga",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    IdCarga = table.Column<Guid>(type: "uuid", nullable: false),
                    Fila = table.Column<int>(type: "integer", nullable: false),
                    CodigoProducto = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Observacion = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    FechaRegistro = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DetalleCarga", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DataProcesada");

            migrationBuilder.DropTable(
                name: "DetalleCarga");
        }
    }
}
