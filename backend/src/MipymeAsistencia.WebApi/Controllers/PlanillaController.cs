using System.IO;
using MediatR;
using MipymeAsistencia.Application.Common.DTOs;
using ClosedXML.Excel;
using MipymeAsistencia.Application.Common.DTOs.Planilla;
using MipymeAsistencia.Application.Features.Planilla.Commands.GenerarPlanilla;
using MipymeAsistencia.Application.Features.Planilla.Queries.GetAllPlanillas;
using MipymeAsistencia.Application.Features.Planilla.Queries.GetPlanillasByEmpleado;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MipymeAsistencia.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PlanillaController : ControllerBase
{
    private readonly IMediator _mediator;

    public PlanillaController(IMediator mediator) => _mediator = mediator;

    /// <summary>
    /// Obtiene todas las planillas del sistema.
    /// Filtros opcionales: periodo (YYYY-MM) y/o empleado.
    /// Solo accesible por Admin.
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<List<PlanillaResponseDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? periodo    = null,
        [FromQuery] int?    idEmpleado = null)
    {
        var data = await _mediator.Send(new GetAllPlanillasQuery
        {
            PeriodoMesAnio = periodo,
            IdEmpleado     = idEmpleado
        });

        return Ok(ApiResponse<List<PlanillaResponseDto>>.Ok(
            data, $"Se encontraron {data.Count} planillas."));
    }

    /// <summary>
    /// Genera la planilla mensual de un empleado aplicando:
    /// INSS 7%, IR tabla progresiva (Ley 822 LCT), horas extras aprobadas
    /// del periodo, aportes patronales y prestaciones sociales.
    /// Solo accesible por Admin.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<PlanillaResponseDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Generar([FromBody] GenerarPlanillaRequestDto request)
    {
        var data = await _mediator.Send(new GenerarPlanillaCommand
        {
            IdEmpleado       = request.IdEmpleado,
            PeriodoMesAnio   = request.PeriodoMesAnio,
            Comisiones       = request.Comisiones,
            Incentivos       = request.Incentivos,
            Embargo          = request.Embargo,
            Sindicato        = request.Sindicato,
            OtrasDeducciones = request.OtrasDeducciones
        });

        return StatusCode(StatusCodes.Status201Created,
            ApiResponse<PlanillaResponseDto>.Created(
                data, $"Planilla del periodo {request.PeriodoMesAnio} generada correctamente."));
    }

    /// <summary>
    /// Exporta a Excel las planillas encontradas en los filtros.
    /// Solo accesible por Admin.
    /// </summary>
    [HttpGet("export")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Export(
        [FromQuery] string? periodo    = null,
        [FromQuery] int?    idEmpleado = null)
    {
        var data = await _mediator.Send(new GetAllPlanillasQuery
        {
            PeriodoMesAnio = periodo,
            IdEmpleado     = idEmpleado
        });

        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Planillas");

        worksheet.Cell(1, 1).Value = "Reporte de planillas";
        worksheet.Range(1, 1, 1, 14).Merge().Style
            .Font.SetBold()
            .Font.SetFontSize(16)
            .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center)
            .Alignment.SetVertical(XLAlignmentVerticalValues.Center);

        worksheet.Cell(2, 1).Value = $"Periodo: {periodo ?? "Todos"}";
        worksheet.Cell(2, 1).Style.Font.SetItalic();
        worksheet.Range(2, 1, 2, 14).Merge();

        var headers = new[]
        {
            "Empleado", "Cargo", "Período", "Salario básico", "Comisiones",
            "Horas extras", "Pago horas extras", "Incentivos", "Total ingresos",
            "INSS laboral", "IR laboral", "Total deducciones", "Neto a pagar",
            "Fecha emisión"
        };

        var headerRow = 4;
        for (var i = 0; i < headers.Length; i++)
        {
            var cell = worksheet.Cell(headerRow, i + 1);
            cell.Value = headers[i];
            cell.Style.Font.SetBold();
            cell.Style.Fill.SetBackgroundColor(XLColor.FromHtml("#2563EB"));
            cell.Style.Font.SetFontColor(XLColor.White);
            cell.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
            cell.Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
            cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        }

        var currentRow = headerRow + 1;
        foreach (var plan in data)
        {
            worksheet.Cell(currentRow, 1).Value = plan.NombreEmpleado;
            worksheet.Cell(currentRow, 2).Value = plan.CargoEmpleado;
            worksheet.Cell(currentRow, 3).Value = plan.PeriodoMesAnio;
            worksheet.Cell(currentRow, 4).Value = plan.SalarioBase;
            worksheet.Cell(currentRow, 5).Value = plan.Comisiones;
            worksheet.Cell(currentRow, 6).Value = plan.TotalHorasExtras;
            worksheet.Cell(currentRow, 7).Value = plan.PagoHorasExtras;
            worksheet.Cell(currentRow, 8).Value = plan.Incentivos;
            worksheet.Cell(currentRow, 9).Value = plan.TotalIngresos;
            worksheet.Cell(currentRow, 10).Value = plan.InssLaboral;
            worksheet.Cell(currentRow, 11).Value = plan.IrLaboral;
            worksheet.Cell(currentRow, 12).Value = plan.TotalDeducciones;
            worksheet.Cell(currentRow, 13).Value = plan.SalarioNeto;
            worksheet.Cell(currentRow, 14).Value = plan.FechaEmision;

            worksheet.Range(currentRow, 4, currentRow, 5).Style.NumberFormat.Format = "#,##0.00";
            worksheet.Range(currentRow, 7, currentRow, 14).Style.NumberFormat.Format = "#,##0.00";
            worksheet.Cell(currentRow, 14).Style.DateFormat.Format = "yyyy-mm-dd";
            worksheet.Range(currentRow, 1, currentRow, 14).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

            currentRow++;
        }

        if (!data.Any())
        {
            worksheet.Cell(currentRow, 1).Value = "No se encontraron planillas para los filtros especificados.";
            worksheet.Range(currentRow, 1, currentRow, headers.Length).Merge();
            worksheet.Cell(currentRow, 1).Style.Font.SetItalic();
        }

        worksheet.Columns().AdjustToContents();
        worksheet.Column(1).Width = 24;
        worksheet.Column(2).Width = 18;
        worksheet.Column(3).Width = 14;
        worksheet.Column(14).Width = 16;

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        stream.Position = 0;

        var fileName = $"planillas_{DateTime.UtcNow:yyyyMMddHHmmss}.xlsx";
        return File(stream.ToArray(),
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            fileName);
    }

    /// <summary>
    /// Obtiene el historial de planillas de un empleado.
    /// Opcionalmente filtra por periodo YYYY-MM.
    /// </summary>
    [HttpGet("empleado/{idEmpleado:int}")]
    [ProducesResponseType(typeof(ApiResponse<List<PlanillaResponseDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByEmpleado(
        int idEmpleado,
        [FromQuery] string? periodo = null)
    {
        var data = await _mediator.Send(new GetPlanillasByEmpleadoQuery
        {
            IdEmpleado     = idEmpleado,
            PeriodoMesAnio = periodo
        });

        return Ok(ApiResponse<List<PlanillaResponseDto>>.Ok(
            data, $"Se encontraron {data.Count} planillas."));
    }
}
