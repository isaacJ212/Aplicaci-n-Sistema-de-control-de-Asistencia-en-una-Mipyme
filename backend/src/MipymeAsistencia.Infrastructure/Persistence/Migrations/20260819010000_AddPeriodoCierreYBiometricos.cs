using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace MipymeAsistencia.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPeriodoCierreYBiometricos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "periodos_cierre_planilla",
                columns: table => new
                {
                    id_periodo_cierre = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    periodo = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: false),
                    fecha_corte_horas_extras = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    fecha_emision_planilla = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    cerrado = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    fecha_cierre_definitivo = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    id_usuario_cierre = table.Column<int>(nullable: true),
                    observaciones = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_periodos_cierre_planilla", x => x.id_periodo_cierre);
                    table.ForeignKey(
                        name: "FK_periodos_cierre_planilla_usuarios",
                        column: x => x.id_usuario_cierre,
                        principalTable: "usuarios",
                        principalColumn: "id_usuario",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "dispositivos_biometricos",
                columns: table => new
                {
                    id_dispositivo = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nombre_dispositivo = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    direccion_ip = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    puerto = table.Column<int>(type: "integer", nullable: false, defaultValue: 4370),
                    tipo_protocolo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "ZKTeco_Standalone"),
                    ubicacion = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    clave_comunicacion = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    activo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    ultima_sincronizacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    estado_conexion = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false, defaultValue: "Desconectado")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dispositivos_biometricos", x => x.id_dispositivo);
                });

            migrationBuilder.CreateTable(
                name: "registros_marcajes_biometricos",
                columns: table => new
                {
                    id_registro_biometrico = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    id_dispositivo = table.Column<int>(nullable: false),
                    numero_enrollamiento = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    fecha_hora = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    tipo_marcaje = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    tipo_verificacion = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false, defaultValue: "Huella"),
                    procesado = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    fecha_procesado = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    id_asistencia_generada = table.Column<int>(nullable: true),
                    error_procesamiento = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_registros_marcajes_biometricos", x => x.id_registro_biometrico);
                    table.ForeignKey(
                        name: "FK_registros_marcajes_biometricos_dispositivos",
                        column: x => x.id_dispositivo,
                        principalTable: "dispositivos_biometricos",
                        principalColumn: "id_dispositivo",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_registros_marcajes_biometricos_asistencia",
                        column: x => x.id_asistencia_generada,
                        principalTable: "historial_asistencia",
                        principalColumn: "id_asistencia",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_periodos_cierre_planilla_periodo",
                table: "periodos_cierre_planilla",
                column: "periodo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_registros_marcajes_biometricos_disp_enroll_fecha",
                table: "registros_marcajes_biometricos",
                columns: new[] { "id_dispositivo", "numero_enrollamiento", "fecha_hora" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "registros_marcajes_biometricos");
            migrationBuilder.DropTable(name: "dispositivos_biometricos");
            migrationBuilder.DropTable(name: "periodos_cierre_planilla");
        }
    }
}
