using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace MipymeAsistencia.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddFeriadosYConfiguracionLaboral : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "dias_feriados",
                columns: table => new
                {
                    id_dia_feriado = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    fecha = table.Column<DateTime>(type: "date", nullable: false),
                    nombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    descripcion = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    es_recuperable = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    es_movil = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dias_feriados", x => x.id_dia_feriado);
                });

            migrationBuilder.CreateTable(
                name: "parametros_laborales",
                columns: table => new
                {
                    id_parametro = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    clave = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    valor = table.Column<decimal>(type: "numeric(10,4)", precision: 10, scale: 4, nullable: false),
                    descripcion = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    fecha_modificacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_parametros_laborales", x => x.id_parametro);
                });

            migrationBuilder.CreateTable(
                name: "tabla_impuesto_renta",
                columns: table => new
                {
                    id_tabla_ir = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    desde_monto_anual = table.Column<decimal>(type: "numeric(14,2)", precision: 14, scale: 2, nullable: false),
                    hasta_monto_anual = table.Column<decimal>(type: "numeric(14,2)", precision: 14, scale: 2, nullable: true),
                    porcentaje_aplicable = table.Column<decimal>(type: "numeric(5,4)", precision: 5, scale: 4, nullable: false),
                    monto_base_exceso = table.Column<decimal>(type: "numeric(14,2)", precision: 14, scale: 2, nullable: false),
                    cuota_fija = table.Column<decimal>(type: "numeric(14,2)", precision: 14, scale: 2, nullable: false),
                    anio_vigencia = table.Column<int>(type: "integer", nullable: false, defaultValue: 2026),
                    activo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tabla_impuesto_renta", x => x.id_tabla_ir);
                });

            migrationBuilder.CreateIndex(
                name: "IX_dias_feriados_fecha",
                table: "dias_feriados",
                column: "fecha",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_parametros_laborales_clave",
                table: "parametros_laborales",
                column: "clave",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "dias_feriados");
            migrationBuilder.DropTable(name: "parametros_laborales");
            migrationBuilder.DropTable(name: "tabla_impuesto_renta");
        }
    }
}
