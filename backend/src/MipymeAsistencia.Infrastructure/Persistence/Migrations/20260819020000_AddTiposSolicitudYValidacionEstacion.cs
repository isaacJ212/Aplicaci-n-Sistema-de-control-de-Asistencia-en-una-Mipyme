using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace MipymeAsistencia.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddTiposSolicitudYValidacionEstacion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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

            migrationBuilder.AddColumn<DateTime>(
                name: "ultima_fecha_login",
                table: "usuarios",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "tipos_solicitud_permiso",
                columns: table => new
                {
                    id_tipo_solicitud = table.Column<int>(nullable: false)
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

            migrationBuilder.CreateIndex(
                name: "IX_tipos_solicitud_permiso_nombre",
                table: "tipos_solicitud_permiso",
                column: "nombre",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "tipos_solicitud_permiso");
            migrationBuilder.DropColumn(name: "ip_estacion_permitida", table: "configuracion_sede");
            migrationBuilder.DropColumn(name: "validar_ip_en_2fa", table: "configuracion_sede");
            migrationBuilder.DropColumn(name: "ultima_ip_login", table: "usuarios");
            migrationBuilder.DropColumn(name: "ultima_mac_login", table: "usuarios");
            migrationBuilder.DropColumn(name: "ultima_fecha_login", table: "usuarios");
        }
    }
}
