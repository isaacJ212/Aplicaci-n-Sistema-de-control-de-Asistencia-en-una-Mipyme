using MediatR;
using MipymeAsistencia.Application.Common.DTOs.Sede;

namespace MipymeAsistencia.Application.Features.Sede.Commands.UpdateSede;

/// <summary>
/// Comando CQRS que actualiza la configuración de la sede existente.
/// Retorna los datos actualizados para confirmar el cambio al cliente.
/// </summary>
public class UpdateSedeCommand : IRequest<SedeResponseDto>
{
    public string NombreSede { get; set; } = string.Empty;
    public decimal LatitudSede { get; set; }
    public decimal LongitudSede { get; set; }
    public int RadioToleranciaMetros { get; set; }

    /// <summary>Formato HH:mm parseado desde el request.</summary>
    public string HoraEntradaOficial { get; set; } = string.Empty;
    public string HoraSalidaOficial { get; set; } = string.Empty;

    public int DuracionAlmuerzoMinutos { get; set; }
    public int MinutosTolerancia { get; set; }
}
