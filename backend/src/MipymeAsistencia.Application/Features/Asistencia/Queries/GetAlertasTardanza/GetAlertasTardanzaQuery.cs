using MediatR;
using MipymeAsistencia.Application.Common.DTOs.Asistencia;

namespace MipymeAsistencia.Application.Features.Asistencia.Queries.GetAlertasTardanza;

/// <summary>
/// Retorna todos los empleados con tardanzas en el período dado.
/// Si UmbralReincidencia = 3, marca como reincidente a quien tuvo ≥ 3 tardanzas.
/// </summary>
public class GetAlertasTardanzaQuery : IRequest<List<AlertaTardanzaDto>>
{
    /// <summary>Formato YYYY-MM. Default: mes actual.</summary>
    public string PeriodoMesAnio     { get; set; } = string.Empty;

    /// <summary>Número de tardanzas a partir del cual se considera reincidente.</summary>
    public int UmbralReincidencia    { get; set; } = 3;
}
