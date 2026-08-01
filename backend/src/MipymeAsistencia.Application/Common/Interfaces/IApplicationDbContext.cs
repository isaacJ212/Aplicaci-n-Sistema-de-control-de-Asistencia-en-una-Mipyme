using MipymeAsistencia.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MipymeAsistencia.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<Usuario> Usuarios { get; }
    DbSet<Rol> Roles { get; }
    DbSet<Empleado> Empleados { get; }
    DbSet<RefreshToken> RefreshTokens { get; }
    DbSet<HistorialAsistencia> HistorialAsistencias { get; }
    DbSet<HistorialPermisoVacacion> HistorialPermisosVacaciones { get; }
    DbSet<HistorialPlanilla> HistorialPlanillas { get; }
    DbSet<HoraExtra> HorasExtras { get; }
    DbSet<EvaluacionDesempeno> EvaluacionesDesempeno { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}