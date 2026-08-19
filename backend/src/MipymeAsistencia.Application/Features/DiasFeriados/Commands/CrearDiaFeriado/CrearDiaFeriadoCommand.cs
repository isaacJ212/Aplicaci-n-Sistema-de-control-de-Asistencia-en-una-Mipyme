using MediatR;
using MipymeAsistencia.Application.Common.DTOs.DiaFeriado;

namespace MipymeAsistencia.Application.Features.DiasFeriados.Commands.CrearDiaFeriado;

public class CrearDiaFeriadoCommand : IRequest<DiaFeriadoDto>
{
    public DateTime Fecha { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public bool EsRecuperable { get; set; } = true;
    public bool EsMovil { get; set; } = false;
}
