using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MipymeAsistencia.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddTardanzaEmbargoSindicatoToHistorialPlanilla : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "minutos_tardanza_mes",
                table: "historial_planillas",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "deduccion_tardanza",
                table: "historial_planillas",
                type: "numeric(12,2)",
                precision: 12,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "embargo",
                table: "historial_planillas",
                type: "numeric(12,2)",
                precision: 12,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "sindicato",
                table: "historial_planillas",
                type: "numeric(12,2)",
                precision: 12,
                scale: 2,
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "minutos_tardanza_mes",
                table: "historial_planillas");

            migrationBuilder.DropColumn(
                name: "deduccion_tardanza",
                table: "historial_planillas");

            migrationBuilder.DropColumn(
                name: "embargo",
                table: "historial_planillas");

            migrationBuilder.DropColumn(
                name: "sindicato",
                table: "historial_planillas");
        }
    }
}
