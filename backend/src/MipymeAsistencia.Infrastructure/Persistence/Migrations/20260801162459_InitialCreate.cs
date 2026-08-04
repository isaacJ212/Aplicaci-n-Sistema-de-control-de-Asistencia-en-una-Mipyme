using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace MipymeAsistencia.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "configuracion_sede",
                columns: table => new
                {
                    id_sede = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nombre_sede = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false, defaultValue: "Sede Principal"),
                    latitud_sede = table.Column<decimal>(type: "numeric(10,8)", precision: 10, scale: 8, nullable: false),
                    longitud_sede = table.Column<decimal>(type: "numeric(11,8)", precision: 11, scale: 8, nullable: false),
                    radio_tolerancia_metros = table.Column<int>(type: "integer", nullable: false, defaultValue: 100),
                    hora_entrada_oficial = table.Column<TimeSpan>(type: "interval", nullable: false),
                    hora_salida_oficial = table.Column<TimeSpan>(type: "interval", nullable: false),
                    duracion_almuerzo_minutos = table.Column<int>(type: "integer", nullable: false, defaultValue: 60),
                    token_qr_actual = table.Column<string>(type: "text", nullable: true),
                    qr_ultima_actualizacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_configuracion_sede", x => x.id_sede);
                });

            migrationBuilder.CreateTable(
                name: "roles",
                columns: table => new
                {
                    id_rol = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nombre_rol = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    descripcion = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_roles", x => x.id_rol);
                });

            migrationBuilder.CreateTable(
                name: "usuarios",
                columns: table => new
                {
                    id_usuario = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    id_rol = table.Column<int>(type: "integer", nullable: false),
                    email = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    password_hash = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    secret_2fa = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    es_2fa_activo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    estado_activo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    fecha_creacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_usuarios", x => x.id_usuario);
                    table.ForeignKey(
                        name: "FK_usuarios_roles_id_rol",
                        column: x => x.id_rol,
                        principalTable: "roles",
                        principalColumn: "id_rol",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "empleados",
                columns: table => new
                {
                    id_empleado = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    id_usuario = table.Column<int>(type: "integer", nullable: false),
                    cedula_identificacion = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    foto_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    nombres = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    apellidos = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    cargo_funcion = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    responsabilidades = table.Column<string>(type: "text", nullable: false),
                    fecha_contratacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    salario_base_mensual = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    dias_vacaciones_acumuladas = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false, defaultValue: 0m)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_empleados", x => x.id_empleado);
                    table.ForeignKey(
                        name: "FK_empleados_usuarios_id_usuario",
                        column: x => x.id_usuario,
                        principalTable: "usuarios",
                        principalColumn: "id_usuario",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "evaluaciones_desempeno",
                columns: table => new
                {
                    id_evaluacion = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    id_empleado = table.Column<int>(type: "integer", nullable: false),
                    id_evaluador = table.Column<int>(type: "integer", nullable: false),
                    periodo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    porcentaje_puntualidad = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: true),
                    calificacion_cumplimiento_funciones = table.Column<int>(type: "integer", nullable: true),
                    observaciones = table.Column<string>(type: "text", nullable: true),
                    fecha_evaluacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_evaluaciones_desempeno", x => x.id_evaluacion);
                    table.ForeignKey(
                        name: "FK_evaluaciones_desempeno_empleados_id_empleado",
                        column: x => x.id_empleado,
                        principalTable: "empleados",
                        principalColumn: "id_empleado",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_evaluaciones_desempeno_usuarios_id_evaluador",
                        column: x => x.id_evaluador,
                        principalTable: "usuarios",
                        principalColumn: "id_usuario",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "historial_asistencia",
                columns: table => new
                {
                    id_asistencia = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    id_empleado = table.Column<int>(type: "integer", nullable: false),
                    fecha = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    hora_entrada = table.Column<TimeSpan>(type: "interval", nullable: false),
                    inicio_almuerzo = table.Column<TimeSpan>(type: "interval", nullable: true),
                    fin_almuerzo = table.Column<TimeSpan>(type: "interval", nullable: true),
                    hora_salida = table.Column<TimeSpan>(type: "interval", nullable: true),
                    latitud_marcaje = table.Column<decimal>(type: "numeric(10,8)", precision: 10, scale: 8, nullable: false),
                    longitud_marcaje = table.Column<decimal>(type: "numeric(11,8)", precision: 11, scale: 8, nullable: false),
                    distancia_calculada_metros = table.Column<decimal>(type: "numeric(8,2)", precision: 8, scale: 2, nullable: false),
                    estado_asistencia = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    minutos_tardanza = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    esta_dentro_del_rango_gps = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_historial_asistencia", x => x.id_asistencia);
                    table.ForeignKey(
                        name: "FK_historial_asistencia_empleados_id_empleado",
                        column: x => x.id_empleado,
                        principalTable: "empleados",
                        principalColumn: "id_empleado",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "historial_permisos_vacaciones",
                columns: table => new
                {
                    id_solicitud = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    id_empleado = table.Column<int>(type: "integer", nullable: false),
                    id_usuario_aprobador = table.Column<int>(type: "integer", nullable: true),
                    tipo_solicitud = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    fecha_inicio = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    fecha_fin = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    dias_solicitados = table.Column<decimal>(type: "numeric(4,1)", precision: 4, scale: 1, nullable: false),
                    motivo = table.Column<string>(type: "text", nullable: false),
                    estado_solicitud = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "Pendiente"),
                    fecha_respuesta = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_historial_permisos_vacaciones", x => x.id_solicitud);
                    table.ForeignKey(
                        name: "FK_historial_permisos_vacaciones_empleados_id_empleado",
                        column: x => x.id_empleado,
                        principalTable: "empleados",
                        principalColumn: "id_empleado",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_historial_permisos_vacaciones_usuarios_id_usuario_aprobador",
                        column: x => x.id_usuario_aprobador,
                        principalTable: "usuarios",
                        principalColumn: "id_usuario",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "historial_planillas",
                columns: table => new
                {
                    id_planilla = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    id_empleado = table.Column<int>(type: "integer", nullable: false),
                    periodo_mes_anio = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: false),
                    salario_base = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    total_horas_extras = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false, defaultValue: 0m),
                    pago_horas_extras = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false, defaultValue: 0m),
                    salario_bruto = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    inss_laboral = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    ir_laboral = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    otras_deducciones = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false, defaultValue: 0m),
                    total_deducciones = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    salario_neto = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    acumulado_aguinaldo = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    fecha_emision = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_historial_planillas", x => x.id_planilla);
                    table.ForeignKey(
                        name: "FK_historial_planillas_empleados_id_empleado",
                        column: x => x.id_empleado,
                        principalTable: "empleados",
                        principalColumn: "id_empleado",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "horas_extras",
                columns: table => new
                {
                    id_hora_extra = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    id_empleado = table.Column<int>(type: "integer", nullable: false),
                    id_usuario_aprobador = table.Column<int>(type: "integer", nullable: true),
                    fecha = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    cantidad_horas = table.Column<decimal>(type: "numeric(4,2)", precision: 4, scale: 2, nullable: false),
                    motivo = table.Column<string>(type: "text", nullable: false),
                    monto_pagar = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    estado = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "Aprobado")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_horas_extras", x => x.id_hora_extra);
                    table.ForeignKey(
                        name: "FK_horas_extras_empleados_id_empleado",
                        column: x => x.id_empleado,
                        principalTable: "empleados",
                        principalColumn: "id_empleado",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_horas_extras_usuarios_id_usuario_aprobador",
                        column: x => x.id_usuario_aprobador,
                        principalTable: "usuarios",
                        principalColumn: "id_usuario",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "validaciones_qr_marcaje",
                columns: table => new
                {
                    id_validacion = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    id_empleado = table.Column<int>(type: "integer", nullable: false),
                    codigo_otp_generado = table.Column<string>(type: "character varying(6)", maxLength: 6, nullable: false),
                    token_qr_escaneado = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    fecha_creacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    fecha_expiracion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    fue_utilizado = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    intentos_fallidos = table.Column<int>(type: "integer", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_validaciones_qr_marcaje", x => x.id_validacion);
                    table.ForeignKey(
                        name: "FK_validaciones_qr_marcaje_empleados_id_empleado",
                        column: x => x.id_empleado,
                        principalTable: "empleados",
                        principalColumn: "id_empleado",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_empleados_cedula_identificacion",
                table: "empleados",
                column: "cedula_identificacion",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_empleados_id_usuario",
                table: "empleados",
                column: "id_usuario",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_evaluaciones_desempeno_id_empleado",
                table: "evaluaciones_desempeno",
                column: "id_empleado");

            migrationBuilder.CreateIndex(
                name: "IX_evaluaciones_desempeno_id_evaluador",
                table: "evaluaciones_desempeno",
                column: "id_evaluador");

            migrationBuilder.CreateIndex(
                name: "IX_historial_asistencia_estado_asistencia",
                table: "historial_asistencia",
                column: "estado_asistencia");

            migrationBuilder.CreateIndex(
                name: "IX_historial_asistencia_id_empleado_fecha",
                table: "historial_asistencia",
                columns: new[] { "id_empleado", "fecha" });

            migrationBuilder.CreateIndex(
                name: "IX_historial_permisos_vacaciones_id_empleado_fecha_inicio",
                table: "historial_permisos_vacaciones",
                columns: new[] { "id_empleado", "fecha_inicio" });

            migrationBuilder.CreateIndex(
                name: "IX_historial_permisos_vacaciones_id_usuario_aprobador",
                table: "historial_permisos_vacaciones",
                column: "id_usuario_aprobador");

            migrationBuilder.CreateIndex(
                name: "IX_historial_planillas_id_empleado",
                table: "historial_planillas",
                column: "id_empleado");

            migrationBuilder.CreateIndex(
                name: "IX_historial_planillas_periodo_mes_anio",
                table: "historial_planillas",
                column: "periodo_mes_anio");

            migrationBuilder.CreateIndex(
                name: "IX_horas_extras_id_empleado",
                table: "horas_extras",
                column: "id_empleado");

            migrationBuilder.CreateIndex(
                name: "IX_horas_extras_id_usuario_aprobador",
                table: "horas_extras",
                column: "id_usuario_aprobador");

            migrationBuilder.CreateIndex(
                name: "IX_roles_nombre_rol",
                table: "roles",
                column: "nombre_rol",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_usuarios_email",
                table: "usuarios",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_usuarios_id_rol",
                table: "usuarios",
                column: "id_rol");

            migrationBuilder.CreateIndex(
                name: "IX_validaciones_qr_marcaje_id_empleado",
                table: "validaciones_qr_marcaje",
                column: "id_empleado");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "configuracion_sede");

            migrationBuilder.DropTable(
                name: "evaluaciones_desempeno");

            migrationBuilder.DropTable(
                name: "historial_asistencia");

            migrationBuilder.DropTable(
                name: "historial_permisos_vacaciones");

            migrationBuilder.DropTable(
                name: "historial_planillas");

            migrationBuilder.DropTable(
                name: "horas_extras");

            migrationBuilder.DropTable(
                name: "validaciones_qr_marcaje");

            migrationBuilder.DropTable(
                name: "empleados");

            migrationBuilder.DropTable(
                name: "usuarios");

            migrationBuilder.DropTable(
                name: "roles");
        }
    }
}
