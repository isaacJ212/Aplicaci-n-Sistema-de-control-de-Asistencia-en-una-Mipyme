using MediatR;
using MipymeAsistencia.Application.Common.DTOs.ConfiguracionLaboral;

namespace MipymeAsistencia.Application.Features.ConfiguracionLaboral.Commands.CrearTablaIr;

public class CrearTablaIrCommand : IRequest<TablaIrDto>
{
    public decimal DesdeMontoAnual { get; set; }
    public decimal? HastaMontoAnual { get; set; }
    public decimal PorcentajeAplicable { get; set; }
    public decimal MontoBaseExceso { get; set; }
    public decimal CuotaFija { get; set; }
    public int AnioVigencia { get; set; } = 2026;
    public bool Activo { get; set; } = true;
}
