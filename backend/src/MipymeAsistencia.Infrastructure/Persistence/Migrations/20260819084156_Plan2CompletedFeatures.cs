using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace MipymeAsistencia.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Plan2CompletedFeatures : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_evaluaciones_desempeno_id_empleado",
                table: "evaluaciones_desempeno");

            migrationBuilder.DropColumn(
                name: "calificacion_cumplimiento_funciones",
                table: "evaluaciones_desempeno");

            migrationBuilder.DropColumn(
                name: "fecha_evaluacion",
                table: "evaluaciones_desempeno");

            migrationBuilder.DropColumn(
                name: "porcentaje_puntualidad",
                table: "evaluaciones_desempeno");

            migrationBuilder.AddColumn<DateTime>(
                name: "ultima_fecha_login",
                table: "usuarios",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ultima_ip_login",
                table: "usuarios",
                type: "character varying(60)",
                maxLength: 60,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ultima_mac_login",
                table: "usuarios",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "HorasSolicitadas",
                table: "historial_permisos_vacaciones",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "estado",
                table: "evaluaciones_desempeno",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Pendiente");

            migrationBuilder.AddColumn<DateTime>(
                name: "fecha_completada",
                table: "evaluaciones_desempeno",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "fecha_creacion",
                table: "evaluaciones_desempeno",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "NOW()");

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
                name: "ip_estacion_permitida",
                table: "configuracion_sede",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true,
                defaultValue: "127.0.0.1,::1,192.168.1.0/24");

            migrationBuilder.AddColumn<bool>(
                name: "validar_ip_en_2fa",
                table: "configuracion_sede",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.CreateTable(
                name: "dias_feriados",
                columns: table => new
                {
                    id_dia_feriado = table.Column<int>(type: "integer", nullable: false)
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
                name: "dispositivos_biometricos",
                columns: table => new
                {
                    id_dispositivo = table.Column<int>(type: "integer", nullable: false)
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
                name: "evaluacion_respuestas",
                columns: table => new
                {
                    id_respuesta = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    id_evaluacion = table.Column<int>(type: "integer", nullable: false),
                    numero_pregunta = table.Column<int>(type: "integer", nullable: false),
                    calificacion = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_evaluacion_respuestas", x => x.id_respuesta);
                    table.ForeignKey(
                        name: "FK_evaluacion_respuestas_evaluaciones_desempeno_id_evaluacion",
                        column: x => x.id_evaluacion,
                        principalTable: "evaluaciones_desempeno",
                        principalColumn: "id_evaluacion",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "parametros_laborales",
                columns: table => new
                {
                    id_parametro = table.Column<int>(type: "integer", nullable: false)
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
                name: "periodos_cierre_planilla",
                columns: table => new
                {
                    id_periodo_cierre = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    periodo = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: false),
                    fecha_corte_horas_extras = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    fecha_emision_planilla = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    cerrado = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    fecha_cierre_definitivo = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    id_usuario_cierre = table.Column<int>(type: "integer", nullable: true),
                    observaciones = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_periodos_cierre_planilla", x => x.id_periodo_cierre);
                    table.ForeignKey(
                        name: "FK_periodos_cierre_planilla_usuarios_id_usuario_cierre",
                        column: x => x.id_usuario_cierre,
                        principalTable: "usuarios",
                        principalColumn: "id_usuario",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "tabla_impuesto_renta",
                columns: table => new
                {
                    id_tabla_ir = table.Column<int>(type: "integer", nullable: false)
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

            migrationBuilder.CreateTable(
                name: "tipos_solicitud_permiso",
                columns: table => new
                {
                    id_tipo_solicitud = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    descripcion = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    requiere_comprobante = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    descuenta_vacaciones = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    permite_por_horas = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    maximo_dias_por_solicitud = table.Column<int>(type: "integer", nullable: true),
                    icono = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true, defaultValue: "calendar"),
                    activo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tipos_solicitud_permiso", x => x.id_tipo_solicitud);
                });

            migrationBuilder.CreateTable(
                name: "registros_marcajes_biometricos",
                columns: table => new
                {
                    id_registro_biometrico = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    id_dispositivo = table.Column<int>(type: "integer", nullable: false),
                    numero_enrollamiento = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    fecha_hora = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    tipo_marcaje = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    tipo_verificacion = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false, defaultValue: "Huella"),
                    procesado = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    fecha_procesado = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    id_asistencia_generada = table.Column<int>(type: "integer", nullable: true),
                    error_procesamiento = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_registros_marcajes_biometricos", x => x.id_registro_biometrico);
                    table.ForeignKey(
                        name: "FK_registros_marcajes_biometricos_dispositivos_biometricos_id_~",
                        column: x => x.id_dispositivo,
                        principalTable: "dispositivos_biometricos",
                        principalColumn: "id_dispositivo",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_registros_marcajes_biometricos_historial_asistencia_id_asis~",
                        column: x => x.id_asistencia_generada,
                        principalTable: "historial_asistencia",
                        principalColumn: "id_asistencia",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.InsertData(
                table: "dias_feriados",
                columns: new[] { "id_dia_feriado", "descripcion", "es_recuperable", "fecha", "nombre" },
                values: new object[] { 1, "Feriado Nacional Obligatorio", true, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Año Nuevo" });

            migrationBuilder.InsertData(
                table: "dias_feriados",
                columns: new[] { "id_dia_feriado", "descripcion", "es_movil", "es_recuperable", "fecha", "nombre" },
                values: new object[,]
                {
                    { 2, "Semana Santa", true, true, new DateTime(2026, 4, 2, 0, 0, 0, 0, DateTimeKind.Utc), "Jueves Santo" },
                    { 3, "Semana Santa", true, true, new DateTime(2026, 4, 3, 0, 0, 0, 0, DateTimeKind.Utc), "Viernes Santo" }
                });

            migrationBuilder.InsertData(
                table: "dias_feriados",
                columns: new[] { "id_dia_feriado", "descripcion", "es_recuperable", "fecha", "nombre" },
                values: new object[,]
                {
                    { 4, "Feriado Nacional Obligatorio", true, new DateTime(2026, 5, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Día Internacional de los Trabajadores" },
                    { 5, "Feriado Nacional", true, new DateTime(2026, 7, 19, 0, 0, 0, 0, DateTimeKind.Utc), "Día de la Revolución" },
                    { 6, "Feriado Local Managua", true, new DateTime(2026, 8, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Santo Domingo de Guzmán (Bajada)" },
                    { 7, "Feriado Local Managua", true, new DateTime(2026, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc), "Santo Domingo de Guzmán (Dejada)" },
                    { 8, "Fiestas Patrias", true, new DateTime(2026, 9, 14, 0, 0, 0, 0, DateTimeKind.Utc), "Batalla de San Jacinto" },
                    { 9, "Fiestas Patrias", true, new DateTime(2026, 9, 15, 0, 0, 0, 0, DateTimeKind.Utc), "Día de la Independencia de Centroamérica" },
                    { 10, "Feriado Nacional", true, new DateTime(2026, 12, 8, 0, 0, 0, 0, DateTimeKind.Utc), "Día de la Inmaculada Concepción de María" },
                    { 11, "Feriado Nacional Obligatorio", true, new DateTime(2026, 12, 25, 0, 0, 0, 0, DateTimeKind.Utc), "Navidad" }
                });

            migrationBuilder.InsertData(
                table: "dispositivos_biometricos",
                columns: new[] { "id_dispositivo", "activo", "clave_comunicacion", "direccion_ip", "estado_conexion", "nombre_dispositivo", "puerto", "tipo_protocolo", "ubicacion", "ultima_sincronizacion" },
                values: new object[,]
                {
                    { 1, true, null, "192.168.1.201", "Conectado", "Reloj Marcador Principal (Recepción)", 4370, "ZKTeco_Standalone", "Entrada Principal / Recepción", null },
                    { 2, true, null, "192.168.1.202", "Desconectado", "Reloj Marcador Taller / Bodega", 4370, "ZKTeco_Standalone", "Acceso Bodega", null }
                });

            migrationBuilder.InsertData(
                table: "parametros_laborales",
                columns: new[] { "id_parametro", "clave", "descripcion", "fecha_modificacion", "valor" },
                values: new object[,]
                {
                    { 1, "INSS_LABORAL", "Aporte INSS laboral del empleado (%)", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 7.00m },
                    { 2, "INSS_PATRONAL", "Aporte INSS patronal de la empresa (%)", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 21.50m },
                    { 3, "INATEC", "Aporte INATEC patronal (%)", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 2.00m },
                    { 4, "HORAS_LABORALES_MES", "Horas laborales mensuales promedio para cálculo de horas extras y tardanzas", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 240.00m },
                    { 5, "TASA_PRESTACIONES_MENSUAL", "Días de provisión mensual para Aguinaldo, Vacaciones e Indemnización", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 2.50m }
                });

            migrationBuilder.InsertData(
                table: "periodos_cierre_planilla",
                columns: new[] { "id_periodo_cierre", "cerrado", "fecha_cierre_definitivo", "fecha_corte_horas_extras", "fecha_emision_planilla", "id_usuario_cierre", "observaciones", "periodo" },
                values: new object[] { 1, true, new DateTime(2026, 5, 30, 18, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 5, 25, 23, 59, 59, 0, DateTimeKind.Utc), new DateTime(2026, 5, 30, 0, 0, 0, 0, DateTimeKind.Utc), null, "Cierre de planilla Mayo 2026 (Rubí del Valle)", "2026-05" });

            migrationBuilder.InsertData(
                table: "periodos_cierre_planilla",
                columns: new[] { "id_periodo_cierre", "fecha_cierre_definitivo", "fecha_corte_horas_extras", "fecha_emision_planilla", "id_usuario_cierre", "observaciones", "periodo" },
                values: new object[] { 2, null, new DateTime(2026, 8, 25, 23, 59, 59, 0, DateTimeKind.Utc), new DateTime(2026, 8, 30, 0, 0, 0, 0, DateTimeKind.Utc), null, "Periodo activo Agosto 2026", "2026-08" });

            migrationBuilder.InsertData(
                table: "tabla_impuesto_renta",
                columns: new[] { "id_tabla_ir", "activo", "anio_vigencia", "cuota_fija", "desde_monto_anual", "hasta_monto_anual", "monto_base_exceso", "porcentaje_aplicable" },
                values: new object[,]
                {
                    { 1, true, 2026, 0.00m, 0.00m, 100000.00m, 0.00m, 0.00m },
                    { 2, true, 2026, 0.00m, 100000.01m, 200000.00m, 100000.00m, 0.15m },
                    { 3, true, 2026, 15000.00m, 200000.01m, 350000.00m, 200000.00m, 0.20m },
                    { 4, true, 2026, 45000.00m, 350000.01m, 500000.00m, 350000.00m, 0.25m },
                    { 5, true, 2026, 82500.00m, 500000.01m, null, 500000.00m, 0.30m }
                });

            migrationBuilder.InsertData(
                table: "tipos_solicitud_permiso",
                columns: new[] { "id_tipo_solicitud", "activo", "descripcion", "descuenta_vacaciones", "icono", "maximo_dias_por_solicitud", "nombre" },
                values: new object[] { 1, true, "Días de descanso anual remunerado con cargo al saldo acumulado", true, "beach_access", 30, "Vacaciones" });

            migrationBuilder.InsertData(
                table: "tipos_solicitud_permiso",
                columns: new[] { "id_tipo_solicitud", "activo", "descripcion", "icono", "maximo_dias_por_solicitud", "nombre", "permite_por_horas", "requiere_comprobante" },
                values: new object[] { 2, true, "Incapacidad médica o cita médica justificada", "local_hospital", 15, "Permiso Médico", true, true });

            migrationBuilder.InsertData(
                table: "tipos_solicitud_permiso",
                columns: new[] { "id_tipo_solicitud", "activo", "descripcion", "icono", "maximo_dias_por_solicitud", "nombre", "permite_por_horas" },
                values: new object[] { 3, true, "Asuntos personales o trámites administrativos", "person", 3, "Permiso Personal", true });

            migrationBuilder.InsertData(
                table: "tipos_solicitud_permiso",
                columns: new[] { "id_tipo_solicitud", "activo", "descripcion", "icono", "maximo_dias_por_solicitud", "nombre", "requiere_comprobante" },
                values: new object[] { 4, true, "Fallecimiento de familiar directo o calamidad doméstica (Arto. 73 Código del Trabajo)", "favorite", 5, "Duelo / Calamidad", true });

            migrationBuilder.InsertData(
                table: "tipos_solicitud_permiso",
                columns: new[] { "id_tipo_solicitud", "activo", "descripcion", "icono", "maximo_dias_por_solicitud", "nombre", "permite_por_horas", "requiere_comprobante" },
                values: new object[] { 5, true, "Permiso por exámenes o capacitaciones laborales autorizadas", "school", 7, "Licencia de Estudio", true, true });

            migrationBuilder.CreateIndex(
                name: "IX_evaluaciones_desempeno_id_empleado_periodo",
                table: "evaluaciones_desempeno",
                columns: new[] { "id_empleado", "periodo" });

            migrationBuilder.CreateIndex(
                name: "IX_dias_feriados_fecha",
                table: "dias_feriados",
                column: "fecha",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_evaluacion_respuestas_id_evaluacion",
                table: "evaluacion_respuestas",
                column: "id_evaluacion");

            migrationBuilder.CreateIndex(
                name: "IX_parametros_laborales_clave",
                table: "parametros_laborales",
                column: "clave",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_periodos_cierre_planilla_id_usuario_cierre",
                table: "periodos_cierre_planilla",
                column: "id_usuario_cierre");

            migrationBuilder.CreateIndex(
                name: "IX_periodos_cierre_planilla_periodo",
                table: "periodos_cierre_planilla",
                column: "periodo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_registros_marcajes_biometricos_id_asistencia_generada",
                table: "registros_marcajes_biometricos",
                column: "id_asistencia_generada");

            migrationBuilder.CreateIndex(
                name: "IX_registros_marcajes_biometricos_id_dispositivo_numero_enroll~",
                table: "registros_marcajes_biometricos",
                columns: new[] { "id_dispositivo", "numero_enrollamiento", "fecha_hora" });

            migrationBuilder.CreateIndex(
                name: "IX_tipos_solicitud_permiso_nombre",
                table: "tipos_solicitud_permiso",
                column: "nombre",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "dias_feriados");

            migrationBuilder.DropTable(
                name: "evaluacion_respuestas");

            migrationBuilder.DropTable(
                name: "parametros_laborales");

            migrationBuilder.DropTable(
                name: "periodos_cierre_planilla");

            migrationBuilder.DropTable(
                name: "registros_marcajes_biometricos");

            migrationBuilder.DropTable(
                name: "tabla_impuesto_renta");

            migrationBuilder.DropTable(
                name: "tipos_solicitud_permiso");

            migrationBuilder.DropTable(
                name: "dispositivos_biometricos");

            migrationBuilder.DropIndex(
                name: "IX_evaluaciones_desempeno_id_empleado_periodo",
                table: "evaluaciones_desempeno");

            migrationBuilder.DropColumn(
                name: "ultima_fecha_login",
                table: "usuarios");

            migrationBuilder.DropColumn(
                name: "ultima_ip_login",
                table: "usuarios");

            migrationBuilder.DropColumn(
                name: "ultima_mac_login",
                table: "usuarios");

            migrationBuilder.DropColumn(
                name: "HorasSolicitadas",
                table: "historial_permisos_vacaciones");

            migrationBuilder.DropColumn(
                name: "estado",
                table: "evaluaciones_desempeno");

            migrationBuilder.DropColumn(
                name: "fecha_completada",
                table: "evaluaciones_desempeno");

            migrationBuilder.DropColumn(
                name: "fecha_creacion",
                table: "evaluaciones_desempeno");

            migrationBuilder.DropColumn(
                name: "perspectiva",
                table: "evaluaciones_desempeno");

            migrationBuilder.DropColumn(
                name: "puntaje_final",
                table: "evaluaciones_desempeno");

            migrationBuilder.DropColumn(
                name: "ip_estacion_permitida",
                table: "configuracion_sede");

            migrationBuilder.DropColumn(
                name: "validar_ip_en_2fa",
                table: "configuracion_sede");

            migrationBuilder.AddColumn<int>(
                name: "calificacion_cumplimiento_funciones",
                table: "evaluaciones_desempeno",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "fecha_evaluacion",
                table: "evaluaciones_desempeno",
                type: "timestamp with time zone",
                nullable: true,
                defaultValueSql: "NOW()");

            migrationBuilder.AddColumn<decimal>(
                name: "porcentaje_puntualidad",
                table: "evaluaciones_desempeno",
                type: "numeric(5,2)",
                precision: 5,
                scale: 2,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_evaluaciones_desempeno_id_empleado",
                table: "evaluaciones_desempeno",
                column: "id_empleado");
        }
    }
}
