using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace MipymeAsistencia.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Evaluacion360 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ── 1. Eliminar columnas obsoletas de evaluaciones_desempeno ──────
            migrationBuilder.DropColumn(
                name: "porcentaje_puntualidad",
                table: "evaluaciones_desempeno");

            migrationBuilder.DropColumn(
                name: "calificacion_cumplimiento_funciones",
                table: "evaluaciones_desempeno");

            // ── 2. Agregar columnas nuevas a evaluaciones_desempeno ───────────
            migrationBuilder.AddColumn<string>(
                name: "perspectiva",
                table: "evaluaciones_desempeno",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "Jefe");

            migrationBuilder.AddColumn<decimal>(
                name: "puntaje_final",
                table: "evaluaciones_desempeno",
                type: "numeric(5,2)",
                precision: 5,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "estado",
                table: "evaluaciones_desempeno",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Pendiente");

            migrationBuilder.AddColumn<DateTime>(
                name: "fecha_creacion",
                table: "evaluaciones_desempeno",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "NOW()");

            migrationBuilder.AddColumn<DateTime>(
                name: "fecha_completada",
                table: "evaluaciones_desempeno",
                type: "timestamp with time zone",
                nullable: true);

            // ── 3. Crear tabla de respuestas 360° ────────────────────────────
            migrationBuilder.CreateTable(
                name: "evaluacion_respuestas",
                columns: table => new
                {
                    id_respuesta    = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    id_evaluacion   = table.Column<int>(nullable: false),
                    numero_pregunta = table.Column<int>(nullable: false),
                    calificacion    = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_evaluacion_respuestas", x => x.id_respuesta);
                    table.ForeignKey(
                        name: "FK_evaluacion_respuestas_evaluaciones_desempeno",
                        column: x => x.id_evaluacion,
                        principalTable: "evaluaciones_desempeno",
                        principalColumn: "id_evaluacion",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_evaluacion_respuestas_id_evaluacion",
                table: "evaluacion_respuestas",
                column: "id_evaluacion");

            migrationBuilder.CreateIndex(
                name: "IX_evaluaciones_desempeno_empleado_periodo",
                table: "evaluaciones_desempeno",
                columns: new[] { "id_empleado", "periodo" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "evaluacion_respuestas");

            migrationBuilder.DropColumn(name: "perspectiva",      table: "evaluaciones_desempeno");
            migrationBuilder.DropColumn(name: "puntaje_final",    table: "evaluaciones_desempeno");
            migrationBuilder.DropColumn(name: "estado",           table: "evaluaciones_desempeno");
            migrationBuilder.DropColumn(name: "fecha_creacion",   table: "evaluaciones_desempeno");
            migrationBuilder.DropColumn(name: "fecha_completada", table: "evaluaciones_desempeno");

            migrationBuilder.AddColumn<decimal>(
                name: "porcentaje_puntualidad",
                table: "evaluaciones_desempeno",
                type: "numeric(5,2)",
                precision: 5,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "calificacion_cumplimiento_funciones",
                table: "evaluaciones_desempeno",
                type: "integer",
                nullable: true);
        }
    }
}
