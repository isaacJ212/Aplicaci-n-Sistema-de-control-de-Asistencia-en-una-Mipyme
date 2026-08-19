using MipymeAsistencia.Application.Common.Interfaces;
using MipymeAsistencia.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MipymeAsistencia.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<ConfiguracionSede> ConfiguracionesSede => Set<ConfiguracionSede>();
    public DbSet<Rol> Roles => Set<Rol>();
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Empleado> Empleados => Set<Empleado>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<ValidacionQrMarcaje> ValidacionesQrMarcaje => Set<ValidacionQrMarcaje>();
    public DbSet<HistorialAsistencia> HistorialAsistencias => Set<HistorialAsistencia>();
    public DbSet<HoraExtra> HorasExtras => Set<HoraExtra>();
    public DbSet<HistorialPermisoVacacion> HistorialPermisosVacaciones => Set<HistorialPermisoVacacion>();
    public DbSet<HistorialPlanilla> HistorialPlanillas => Set<HistorialPlanilla>();
    public DbSet<EvaluacionDesempeno> EvaluacionesDesempeno => Set<EvaluacionDesempeno>();
    public DbSet<EvaluacionRespuesta> EvaluacionRespuestas  => Set<EvaluacionRespuesta>();
    public DbSet<DiaFeriado> DiasFeriados => Set<DiaFeriado>();
    public DbSet<ParametroLaboral> ParametrosLaborales => Set<ParametroLaboral>();
    public DbSet<TablaImpuestoRenta> TablaImpuestoRenta => Set<TablaImpuestoRenta>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ConfiguracionSede>(entity =>
        {
            entity.ToTable("configuracion_sede");
            entity.HasKey(x => x.IdSede);
            entity.Property(x => x.IdSede).HasColumnName("id_sede");
            entity.Property(x => x.NombreSede).HasColumnName("nombre_sede").HasMaxLength(100).HasDefaultValue("Sede Principal");
            entity.Property(x => x.LatitudSede).HasColumnName("latitud_sede").HasPrecision(10, 8);
            entity.Property(x => x.LongitudSede).HasColumnName("longitud_sede").HasPrecision(11, 8);
            entity.Property(x => x.RadioToleranciaMetros).HasColumnName("radio_tolerancia_metros").HasDefaultValue(100);
            entity.Property(x => x.HoraEntradaOficial).HasColumnName("hora_entrada_oficial");
            entity.Property(x => x.HoraSalidaOficial).HasColumnName("hora_salida_oficial");
            entity.Property(x => x.DuracionAlmuerzoMinutos).HasColumnName("duracion_almuerzo_minutos").HasDefaultValue(60);
            entity.Property(x => x.MinutosTolerancia).HasColumnName("minutos_tolerancia").HasDefaultValue(10);
            entity.Property(x => x.TokenQrActual).HasColumnName("token_qr_actual");
            entity.Property(x => x.QrUltimaActualizacion).HasColumnName("qr_ultima_actualizacion");
        });

        modelBuilder.Entity<Rol>(entity =>
        {
            entity.ToTable("roles");
            entity.HasKey(x => x.IdRol);
            entity.Property(x => x.IdRol).HasColumnName("id_rol");
            entity.Property(x => x.NombreRol).HasColumnName("nombre_rol").HasMaxLength(50);
            entity.Property(x => x.Descripcion).HasColumnName("descripcion").HasMaxLength(255);
            entity.HasIndex(x => x.NombreRol).IsUnique();
        });

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.ToTable("usuarios");
            entity.HasKey(x => x.IdUsuario);
            entity.Property(x => x.IdUsuario).HasColumnName("id_usuario");
            entity.Property(x => x.IdRol).HasColumnName("id_rol");
            entity.Property(x => x.Email).HasColumnName("email").HasMaxLength(100);
            entity.Property(x => x.PasswordHash).HasColumnName("password_hash").HasMaxLength(255);
            entity.Property(x => x.Secret2Fa).HasColumnName("secret_2fa").HasMaxLength(255);
            entity.Property(x => x.Es2FaActivo).HasColumnName("es_2fa_activo").HasDefaultValue(false);
            entity.Property(x => x.EstadoActivo).HasColumnName("estado_activo").HasDefaultValue(true);
            entity.Property(x => x.FechaCreacion).HasColumnName("fecha_creacion").HasDefaultValueSql("NOW()");

            entity.HasIndex(x => x.Email).IsUnique();

            entity.HasOne(x => x.Rol)
                .WithMany(x => x.Usuarios)
                .HasForeignKey(x => x.IdRol)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Empleado>(entity =>
        {
            entity.ToTable("empleados");
            entity.HasKey(x => x.IdEmpleado);
            entity.Property(x => x.IdEmpleado).HasColumnName("id_empleado");
            entity.Property(x => x.IdUsuario).HasColumnName("id_usuario");
            entity.Property(x => x.CedulaIdentificacion).HasColumnName("cedula_identificacion").HasMaxLength(20);
            entity.Property(x => x.NumeroInss).HasColumnName("numero_inss").HasMaxLength(20).HasDefaultValue(string.Empty);
            entity.Property(x => x.EstadoCivil).HasColumnName("estado_civil").HasMaxLength(30).HasDefaultValue("Soltero");
            entity.Property(x => x.EstadoEmpleado).HasColumnName("estado_empleado").HasMaxLength(30).HasDefaultValue("Activo");
            entity.Property(x => x.FotoUrl).HasColumnName("foto_url").HasMaxLength(500);
            entity.Property(x => x.Nombres).HasColumnName("nombres").HasMaxLength(100);
            entity.Property(x => x.Apellidos).HasColumnName("apellidos").HasMaxLength(100);
            entity.Property(x => x.CargoFuncion).HasColumnName("cargo_funcion").HasMaxLength(100);
            entity.Property(x => x.Responsabilidades).HasColumnName("responsabilidades");
            entity.Property(x => x.FechaContratacion).HasColumnName("fecha_contratacion");
            entity.Property(x => x.SalarioBaseMensual).HasColumnName("salario_base_mensual").HasPrecision(12, 2);
            entity.Property(x => x.DiasVacacionesAcumuladas).HasColumnName("dias_vacaciones_acumuladas").HasPrecision(5, 2).HasDefaultValue(0m);

            entity.HasIndex(x => x.CedulaIdentificacion).IsUnique();

            entity.HasOne(x => x.Usuario)
                .WithOne(x => x.Empleado)
                .HasForeignKey<Empleado>(x => x.IdUsuario)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ValidacionQrMarcaje>(entity =>
        {
            entity.ToTable("validaciones_qr_marcaje");
            entity.HasKey(x => x.IdValidacion);
            entity.Property(x => x.IdValidacion).HasColumnName("id_validacion");
            entity.Property(x => x.IdEmpleado).HasColumnName("id_empleado");
            entity.Property(x => x.CodigoOtpGenerado).HasColumnName("codigo_otp_generado").HasMaxLength(6);
            entity.Property(x => x.TokenQrEscaneado).HasColumnName("token_qr_escaneado").HasMaxLength(255);
            entity.Property(x => x.FechaCreacion).HasColumnName("fecha_creacion").HasDefaultValueSql("NOW()");
            entity.Property(x => x.FechaExpiracion).HasColumnName("fecha_expiracion");
            entity.Property(x => x.FueUtilizado).HasColumnName("fue_utilizado").HasDefaultValue(false);
            entity.Property(x => x.IntentosFallidos).HasColumnName("intentos_fallidos").HasDefaultValue(0);

            entity.HasOne(x => x.Empleado)
                .WithMany(x => x.ValidacionesQrMarcaje)
                .HasForeignKey(x => x.IdEmpleado)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<HistorialAsistencia>(entity =>
        {
            entity.ToTable("historial_asistencia");
            entity.HasKey(x => x.IdAsistencia);
            entity.Property(x => x.IdAsistencia).HasColumnName("id_asistencia");
            entity.Property(x => x.IdEmpleado).HasColumnName("id_empleado");
            entity.Property(x => x.Fecha).HasColumnName("fecha");
            entity.Property(x => x.HoraEntrada).HasColumnName("hora_entrada");
            entity.Property(x => x.InicioAlmuerzo).HasColumnName("inicio_almuerzo");
            entity.Property(x => x.FinAlmuerzo).HasColumnName("fin_almuerzo");
            entity.Property(x => x.HoraSalida).HasColumnName("hora_salida");
            entity.Property(x => x.LatitudMarcaje).HasColumnName("latitud_marcaje").HasPrecision(10, 8);
            entity.Property(x => x.LongitudMarcaje).HasColumnName("longitud_marcaje").HasPrecision(11, 8);
            entity.Property(x => x.DistanciaCalculadaMetros).HasColumnName("distancia_calculada_metros").HasPrecision(8, 2);
            entity.Property(x => x.EstadoAsistencia).HasColumnName("estado_asistencia").HasMaxLength(20);
            entity.Property(x => x.MinutosTardanza).HasColumnName("minutos_tardanza").HasDefaultValue(0);
            entity.Property(x => x.EstaDentroDelRangoGps).HasColumnName("esta_dentro_del_rango_gps").HasDefaultValue(true);

            entity.HasOne(x => x.Empleado)
                .WithMany(x => x.HistorialAsistencias)
                .HasForeignKey(x => x.IdEmpleado)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(x => new { x.IdEmpleado, x.Fecha });
            entity.HasIndex(x => x.EstadoAsistencia);
        });

        modelBuilder.Entity<HoraExtra>(entity =>
        {
            entity.ToTable("horas_extras");
            entity.HasKey(x => x.IdHoraExtra);
            entity.Property(x => x.IdHoraExtra).HasColumnName("id_hora_extra");
            entity.Property(x => x.IdEmpleado).HasColumnName("id_empleado");
            entity.Property(x => x.IdUsuarioAprobador).HasColumnName("id_usuario_aprobador");
            entity.Property(x => x.Fecha).HasColumnName("fecha");
            entity.Property(x => x.CantidadHoras).HasColumnName("cantidad_horas").HasPrecision(4, 2);
            entity.Property(x => x.Motivo).HasColumnName("motivo");
            entity.Property(x => x.MontoPagar).HasColumnName("monto_pagar").HasPrecision(12, 2);
            entity.Property(x => x.Estado).HasColumnName("estado").HasMaxLength(20).HasDefaultValue("Aprobado");

            entity.HasOne(x => x.Empleado)
                .WithMany(x => x.HorasExtras)
                .HasForeignKey(x => x.IdEmpleado)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.UsuarioAprobador)
                .WithMany(x => x.HorasExtrasAprobadas)
                .HasForeignKey(x => x.IdUsuarioAprobador)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<HistorialPermisoVacacion>(entity =>
        {
            entity.ToTable("historial_permisos_vacaciones");
            entity.HasKey(x => x.IdSolicitud);
            entity.Property(x => x.IdSolicitud).HasColumnName("id_solicitud");
            entity.Property(x => x.IdEmpleado).HasColumnName("id_empleado");
            entity.Property(x => x.IdUsuarioAprobador).HasColumnName("id_usuario_aprobador");
            entity.Property(x => x.TipoSolicitud).HasColumnName("tipo_solicitud").HasMaxLength(30);
            entity.Property(x => x.FechaInicio).HasColumnName("fecha_inicio");
            entity.Property(x => x.FechaFin).HasColumnName("fecha_fin");
            entity.Property(x => x.DiasSolicitados).HasColumnName("dias_solicitados").HasPrecision(4, 1);
            entity.Property(x => x.Motivo).HasColumnName("motivo");
            entity.Property(x => x.EstadoSolicitud).HasColumnName("estado_solicitud").HasMaxLength(20).HasDefaultValue("Pendiente");
            entity.Property(x => x.FechaRespuesta).HasColumnName("fecha_respuesta");

            entity.HasOne(x => x.Empleado)
                .WithMany(x => x.Solicitudes)
                .HasForeignKey(x => x.IdEmpleado)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.UsuarioAprobador)
                .WithMany(x => x.PermisosAprobados)
                .HasForeignKey(x => x.IdUsuarioAprobador)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasIndex(x => new { x.IdEmpleado, x.FechaInicio });
        });

        modelBuilder.Entity<HistorialPlanilla>(entity =>
        {
            entity.ToTable("historial_planillas");
            entity.HasKey(x => x.IdPlanilla);
            entity.Property(x => x.IdPlanilla).HasColumnName("id_planilla");
            entity.Property(x => x.IdEmpleado).HasColumnName("id_empleado");
            entity.Property(x => x.PeriodoMesAnio).HasColumnName("periodo_mes_anio").HasMaxLength(7);
            entity.Property(x => x.SalarioBase).HasColumnName("salario_base").HasPrecision(12, 2);
            entity.Property(x => x.TotalHorasExtras).HasColumnName("total_horas_extras").HasPrecision(5, 2).HasDefaultValue(0m);
            entity.Property(x => x.PagoHorasExtras).HasColumnName("pago_horas_extras").HasPrecision(12, 2).HasDefaultValue(0m);
            entity.Property(x => x.SalarioBruto).HasColumnName("salario_bruto").HasPrecision(12, 2);
            entity.Property(x => x.InssLaboral).HasColumnName("inss_laboral").HasPrecision(12, 2);
            entity.Property(x => x.IrLaboral).HasColumnName("ir_laboral").HasPrecision(12, 2);
            entity.Property(x => x.MinutosTardanzaMes).HasColumnName("minutos_tardanza_mes").HasDefaultValue(0);
            entity.Property(x => x.DeduccionTardanza).HasColumnName("deduccion_tardanza").HasPrecision(12, 2).HasDefaultValue(0m);
            entity.Property(x => x.Embargo).HasColumnName("embargo").HasPrecision(12, 2).HasDefaultValue(0m);
            entity.Property(x => x.Sindicato).HasColumnName("sindicato").HasPrecision(12, 2).HasDefaultValue(0m);
            entity.Property(x => x.OtrasDeducciones).HasColumnName("otras_deducciones").HasPrecision(12, 2).HasDefaultValue(0m);
            entity.Property(x => x.TotalDeducciones).HasColumnName("total_deducciones").HasPrecision(12, 2);
            entity.Property(x => x.SalarioNeto).HasColumnName("salario_neto").HasPrecision(12, 2);
            entity.Property(x => x.AcumuladoAguinaldo).HasColumnName("acumulado_aguinaldo").HasPrecision(12, 2);
            entity.Property(x => x.FechaEmision).HasColumnName("fecha_emision");

            entity.HasOne(x => x.Empleado)
                .WithMany(x => x.Planillas)
                .HasForeignKey(x => x.IdEmpleado)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(x => x.PeriodoMesAnio);
        });

        modelBuilder.Entity<EvaluacionDesempeno>(entity =>
        {
            entity.ToTable("evaluaciones_desempeno");
            entity.HasKey(x => x.IdEvaluacion);
            entity.Property(x => x.IdEvaluacion).HasColumnName("id_evaluacion");
            entity.Property(x => x.IdEmpleado).HasColumnName("id_empleado");
            entity.Property(x => x.IdEvaluador).HasColumnName("id_evaluador");
            entity.Property(x => x.Perspectiva).HasColumnName("perspectiva").HasMaxLength(30).HasDefaultValue("Jefe");
            entity.Property(x => x.Periodo).HasColumnName("periodo").HasMaxLength(20);
            entity.Property(x => x.PuntajeFinal).HasColumnName("puntaje_final").HasPrecision(5, 2).HasDefaultValue(0m);
            entity.Property(x => x.Observaciones).HasColumnName("observaciones");
            entity.Property(x => x.Estado).HasColumnName("estado").HasMaxLength(20).HasDefaultValue("Pendiente");
            entity.Property(x => x.FechaCreacion).HasColumnName("fecha_creacion").HasDefaultValueSql("NOW()");
            entity.Property(x => x.FechaCompletada).HasColumnName("fecha_completada");

            entity.HasOne(x => x.Empleado)
                .WithMany(x => x.Evaluaciones)
                .HasForeignKey(x => x.IdEmpleado)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.Evaluador)
                .WithMany(x => x.EvaluacionesRealizadas)
                .HasForeignKey(x => x.IdEvaluador)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => new { x.IdEmpleado, x.Periodo });
        });

        modelBuilder.Entity<EvaluacionRespuesta>(entity =>
        {
            entity.ToTable("evaluacion_respuestas");
            entity.HasKey(x => x.IdRespuesta);
            entity.Property(x => x.IdRespuesta).HasColumnName("id_respuesta");
            entity.Property(x => x.IdEvaluacion).HasColumnName("id_evaluacion");
            entity.Property(x => x.NumeroPregunta).HasColumnName("numero_pregunta");
            entity.Property(x => x.Calificacion).HasColumnName("calificacion");

            entity.HasOne(x => x.Evaluacion)
                .WithMany(x => x.Respuestas)
                .HasForeignKey(x => x.IdEvaluacion)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(x => x.IdEvaluacion);
        });

        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.ToTable("refresh_tokens");
            entity.HasKey(x => x.IdRefreshToken);
            entity.Property(x => x.IdRefreshToken).HasColumnName("id_refresh_token");
            entity.Property(x => x.IdUsuario).HasColumnName("id_usuario");
            entity.Property(x => x.Token).HasColumnName("token").HasMaxLength(512);
            entity.Property(x => x.FechaExpiracion).HasColumnName("fecha_expiracion");
            entity.Property(x => x.FechaCreacion).HasColumnName("fecha_creacion").HasDefaultValueSql("NOW()");
            entity.Property(x => x.FueUtilizado).HasColumnName("fue_utilizado").HasDefaultValue(false);
            entity.Property(x => x.FueRevocado).HasColumnName("fue_revocado").HasDefaultValue(false);

            entity.HasIndex(x => x.Token).IsUnique();
            entity.HasIndex(x => x.IdUsuario);

            entity.HasOne(x => x.Usuario)
                .WithMany(x => x.RefreshTokens)
                .HasForeignKey(x => x.IdUsuario)
                .OnDelete(DeleteBehavior.Cascade);

            // EsActivo es una propiedad calculada — no se mapea a columna
            entity.Ignore(x => x.EsActivo);
        });

        modelBuilder.Entity<DiaFeriado>(entity =>
        {
            entity.ToTable("dias_feriados");
            entity.HasKey(x => x.IdDiaFeriado);
            entity.Property(x => x.IdDiaFeriado).HasColumnName("id_dia_feriado");
            entity.Property(x => x.Fecha).HasColumnName("fecha").HasColumnType("date");
            entity.Property(x => x.Nombre).HasColumnName("nombre").HasMaxLength(100);
            entity.Property(x => x.Descripcion).HasColumnName("descripcion").HasMaxLength(255);
            entity.Property(x => x.EsRecuperable).HasColumnName("es_recuperable").HasDefaultValue(true);
            entity.Property(x => x.EsMovil).HasColumnName("es_movil").HasDefaultValue(false);

            entity.HasIndex(x => x.Fecha).IsUnique();

            entity.HasData(
                new DiaFeriado { IdDiaFeriado = 1, Fecha = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc), Nombre = "Año Nuevo", Descripcion = "Feriado Nacional Obligatorio", EsRecuperable = true, EsMovil = false },
                new DiaFeriado { IdDiaFeriado = 2, Fecha = new DateTime(2026, 4, 2, 0, 0, 0, DateTimeKind.Utc), Nombre = "Jueves Santo", Descripcion = "Semana Santa", EsRecuperable = true, EsMovil = true },
                new DiaFeriado { IdDiaFeriado = 3, Fecha = new DateTime(2026, 4, 3, 0, 0, 0, DateTimeKind.Utc), Nombre = "Viernes Santo", Descripcion = "Semana Santa", EsRecuperable = true, EsMovil = true },
                new DiaFeriado { IdDiaFeriado = 4, Fecha = new DateTime(2026, 5, 1, 0, 0, 0, DateTimeKind.Utc), Nombre = "Día Internacional de los Trabajadores", Descripcion = "Feriado Nacional Obligatorio", EsRecuperable = true, EsMovil = false },
                new DiaFeriado { IdDiaFeriado = 5, Fecha = new DateTime(2026, 7, 19, 0, 0, 0, DateTimeKind.Utc), Nombre = "Día de la Revolución", Descripcion = "Feriado Nacional", EsRecuperable = true, EsMovil = false },
                new DiaFeriado { IdDiaFeriado = 6, Fecha = new DateTime(2026, 8, 1, 0, 0, 0, DateTimeKind.Utc), Nombre = "Santo Domingo de Guzmán (Bajada)", Descripcion = "Feriado Local Managua", EsRecuperable = true, EsMovil = false },
                new DiaFeriado { IdDiaFeriado = 7, Fecha = new DateTime(2026, 8, 10, 0, 0, 0, DateTimeKind.Utc), Nombre = "Santo Domingo de Guzmán (Dejada)", Descripcion = "Feriado Local Managua", EsRecuperable = true, EsMovil = false },
                new DiaFeriado { IdDiaFeriado = 8, Fecha = new DateTime(2026, 9, 14, 0, 0, 0, DateTimeKind.Utc), Nombre = "Batalla de San Jacinto", Descripcion = "Fiestas Patrias", EsRecuperable = true, EsMovil = false },
                new DiaFeriado { IdDiaFeriado = 9, Fecha = new DateTime(2026, 9, 15, 0, 0, 0, DateTimeKind.Utc), Nombre = "Día de la Independencia de Centroamérica", Descripcion = "Fiestas Patrias", EsRecuperable = true, EsMovil = false },
                new DiaFeriado { IdDiaFeriado = 10, Fecha = new DateTime(2026, 12, 8, 0, 0, 0, DateTimeKind.Utc), Nombre = "Día de la Inmaculada Concepción de María", Descripcion = "Feriado Nacional", EsRecuperable = true, EsMovil = false },
                new DiaFeriado { IdDiaFeriado = 11, Fecha = new DateTime(2026, 12, 25, 0, 0, 0, DateTimeKind.Utc), Nombre = "Navidad", Descripcion = "Feriado Nacional Obligatorio", EsRecuperable = true, EsMovil = false }
            );
        });

        modelBuilder.Entity<ParametroLaboral>(entity =>
        {
            entity.ToTable("parametros_laborales");
            entity.HasKey(x => x.IdParametro);
            entity.Property(x => x.IdParametro).HasColumnName("id_parametro");
            entity.Property(x => x.Clave).HasColumnName("clave").HasMaxLength(50);
            entity.Property(x => x.Valor).HasColumnName("valor").HasPrecision(10, 4);
            entity.Property(x => x.Descripcion).HasColumnName("descripcion").HasMaxLength(255);
            entity.Property(x => x.FechaModificacion).HasColumnName("fecha_modificacion").HasDefaultValueSql("NOW()");

            entity.HasIndex(x => x.Clave).IsUnique();

            entity.HasData(
                new ParametroLaboral { IdParametro = 1, Clave = "INSS_LABORAL", Valor = 7.00m, Descripcion = "Aporte INSS laboral del empleado (%)", FechaModificacion = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new ParametroLaboral { IdParametro = 2, Clave = "INSS_PATRONAL", Valor = 21.50m, Descripcion = "Aporte INSS patronal de la empresa (%)", FechaModificacion = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new ParametroLaboral { IdParametro = 3, Clave = "INATEC", Valor = 2.00m, Descripcion = "Aporte INATEC patronal (%)", FechaModificacion = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new ParametroLaboral { IdParametro = 4, Clave = "HORAS_LABORALES_MES", Valor = 240.00m, Descripcion = "Horas laborales mensuales promedio para cálculo de horas extras y tardanzas", FechaModificacion = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new ParametroLaboral { IdParametro = 5, Clave = "TASA_PRESTACIONES_MENSUAL", Valor = 2.50m, Descripcion = "Días de provisión mensual para Aguinaldo, Vacaciones e Indemnización", FechaModificacion = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) }
            );
        });

        modelBuilder.Entity<TablaImpuestoRenta>(entity =>
        {
            entity.ToTable("tabla_impuesto_renta");
            entity.HasKey(x => x.IdTablaIr);
            entity.Property(x => x.IdTablaIr).HasColumnName("id_tabla_ir");
            entity.Property(x => x.DesdeMontoAnual).HasColumnName("desde_monto_anual").HasPrecision(14, 2);
            entity.Property(x => x.HastaMontoAnual).HasColumnName("hasta_monto_anual").HasPrecision(14, 2);
            entity.Property(x => x.PorcentajeAplicable).HasColumnName("porcentaje_aplicable").HasPrecision(5, 4);
            entity.Property(x => x.MontoBaseExceso).HasColumnName("monto_base_exceso").HasPrecision(14, 2);
            entity.Property(x => x.CuotaFija).HasColumnName("cuota_fija").HasPrecision(14, 2);
            entity.Property(x => x.AnioVigencia).HasColumnName("anio_vigencia").HasDefaultValue(2026);
            entity.Property(x => x.Activo).HasColumnName("activo").HasDefaultValue(true);

            entity.HasData(
                new TablaImpuestoRenta { IdTablaIr = 1, DesdeMontoAnual = 0.00m, HastaMontoAnual = 100000.00m, PorcentajeAplicable = 0.00m, MontoBaseExceso = 0.00m, CuotaFija = 0.00m, AnioVigencia = 2026, Activo = true },
                new TablaImpuestoRenta { IdTablaIr = 2, DesdeMontoAnual = 100000.01m, HastaMontoAnual = 200000.00m, PorcentajeAplicable = 0.15m, MontoBaseExceso = 100000.00m, CuotaFija = 0.00m, AnioVigencia = 2026, Activo = true },
                new TablaImpuestoRenta { IdTablaIr = 3, DesdeMontoAnual = 200000.01m, HastaMontoAnual = 350000.00m, PorcentajeAplicable = 0.20m, MontoBaseExceso = 200000.00m, CuotaFija = 15000.00m, AnioVigencia = 2026, Activo = true },
                new TablaImpuestoRenta { IdTablaIr = 4, DesdeMontoAnual = 350000.01m, HastaMontoAnual = 500000.00m, PorcentajeAplicable = 0.25m, MontoBaseExceso = 350000.00m, CuotaFija = 45000.00m, AnioVigencia = 2026, Activo = true },
                new TablaImpuestoRenta { IdTablaIr = 5, DesdeMontoAnual = 500000.01m, HastaMontoAnual = null, PorcentajeAplicable = 0.30m, MontoBaseExceso = 500000.00m, CuotaFija = 82500.00m, AnioVigencia = 2026, Activo = true }
            );
        });

        base.OnModelCreating(modelBuilder);
    }
}
