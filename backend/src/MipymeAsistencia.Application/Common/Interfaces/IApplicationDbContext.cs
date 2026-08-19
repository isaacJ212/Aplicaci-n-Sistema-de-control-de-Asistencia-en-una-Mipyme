using MipymeAsistencia.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MipymeAsistencia.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<ConfiguracionSede> ConfiguracionesSede { get; }
    DbSet<Usuario> Usuarios { get; }
    DbSet<Rol> Roles { get; }
    DbSet<Empleado> Empleados { get; }
    DbSet<ValidacionQrMarcaje> ValidacionesQrMarcaje { get; }
    DbSet<RefreshToken> RefreshTokens { get; }
    DbSet<HistorialAsistencia> HistorialAsistencias { get; }
    DbSet<HistorialPermisoVacacion> HistorialPermisosVacaciones { get; }
    DbSet<HistorialPlanilla> HistorialPlanillas { get; }
    DbSet<HoraExtra> HorasExtras { get; }
    DbSet<EvaluacionDesempeno> EvaluacionesDesempeno { get; }
    DbSet<EvaluacionRespuesta> EvaluacionRespuestas { get; }
    DbSet<DiaFeriado> DiasFeriados { get; }
    DbSet<ParametroLaboral> ParametrosLaborales { get; }
    DbSet<TablaImpuestoRenta> TablaImpuestoRenta { get; }
    DbSet<PeriodoCierrePlanilla> PeriodosCierrePlanilla { get; }
    DbSet<DispositivoBiometrico> DispositivosBiometricos { get; }
    DbSet<RegistroMarcajeBiometrico> RegistrosMarcajesBiometricos { get; }
    DbSet<TipoSolicitudPermiso> TiposSolicitudPermiso { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}