using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MipymeAsistencia.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class FaltanteRequirements : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "estado_civil",
                table: "empleados",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "Soltero");

            migrationBuilder.AddColumn<string>(
                name: "estado_empleado",
                table: "empleados",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "Activo");

            migrationBuilder.AddColumn<string>(
                name: "numero_inss",
                table: "empleados",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "estado_civil",
                table: "empleados");

            migrationBuilder.DropColumn(
                name: "estado_empleado",
                table: "empleados");

            migrationBuilder.DropColumn(
                name: "numero_inss",
                table: "empleados");
        }
    }
}
