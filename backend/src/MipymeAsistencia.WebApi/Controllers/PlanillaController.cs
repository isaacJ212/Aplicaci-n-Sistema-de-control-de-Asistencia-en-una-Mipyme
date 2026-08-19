using System.IO;
using MediatR;
using MipymeAsistencia.Application.Common.DTOs;
using ClosedXML.Excel;
using MipymeAsistencia.Application.Common.DTOs.Planilla;
using MipymeAsistencia.Application.Common.Interfaces;
using MipymeAsistencia.Application.Features.Planilla.Commands.GenerarPlanilla;
using MipymeAsistencia.Application.Features.Planilla.Commands.GenerarPlanillaPorDepartamento;
using MipymeAsistencia.Application.Features.Planilla.Queries.GetAllPlanillas;
using MipymeAsistencia.Application.Features.Planilla.Queries.GetPlanillasByEmpleado;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MipymeAsistencia.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PlanillaController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IApplicationDbContext _context;

    public PlanillaController(IMediator mediator, IApplicationDbContext context)
    {
        _mediator = mediator;
        _context = context;
    }

    /// <summary>
    /// Obtiene todas las planillas del sistema.
    /// Filtros opcionales: periodo (YYYY-MM), departamento y/o empleado.
    /// Solo accesible por Admin.
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<List<PlanillaResponseDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? periodo      = null,
        [FromQuery] string? departamento = null,
        [FromQuery] int?    idEmpleado   = null)
    {
        var data = await _mediator.Send(new GetAllPlanillasQuery
        {
            PeriodoMesAnio = periodo,
            Departamento   = departamento,
            IdEmpleado     = idEmpleado
        });

        return Ok(ApiResponse<List<PlanillaResponseDto>>.Ok(
            data, $"Se encontraron {data.Count} planillas."));
    }

    /// <summary>
    /// Obtiene la lista de departamentos distintos de empleados registrados.
    /// Solo accesible por Admin.
    /// </summary>
    [HttpGet("departamentos")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<List<string>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDepartamentos()
    {
        var departamentos = await _context.Empleados
            .Where(e => !string.IsNullOrEmpty(e.Departamento))
            .Select(e => e.Departamento)
            .Distinct()
            .OrderBy(d => d)
            .ToListAsync();

        return Ok(ApiResponse<List<string>>.Ok(departamentos, "Lista de departamentos"));
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
    /// Genera masivamente las planillas de todos los empleados activos de un departamento (o de todos los departamentos).
    /// Solo accesible por Admin.
    /// </summary>
    [HttpPost("generar-por-departamento")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<GenerarPlanillaPorDepartamentoResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GenerarPorDepartamento([FromBody] GenerarPlanillaPorDepartamentoRequestDto request)
    {
        var data = await _mediator.Send(new GenerarPlanillaPorDepartamentoCommand
        {
            PeriodoMesAnio            = request.PeriodoMesAnio,
            Departamento              = request.Departamento,
            ComisionesGenerales       = request.ComisionesGenerales,
            IncentivosGenerales       = request.IncentivosGenerales,
            OtrasDeduccionesGenerales = request.OtrasDeduccionesGenerales
        });

        return Ok(ApiResponse<GenerarPlanillaPorDepartamentoResponseDto>.Ok(data, data.Mensaje));
    }

    /// <summary>
    /// Exporta a Excel las planillas encontradas en los filtros.
    /// Solo accesible por Admin.
    /// </summary>
    [HttpGet("export")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Export(
        [FromQuery] string? periodo      = null,
        [FromQuery] string? departamento = null,
        [FromQuery] int?    idEmpleado   = null)
    {
        var data = await _mediator.Send(new GetAllPlanillasQuery
        {
            PeriodoMesAnio = periodo,
            Departamento   = departamento,
            IdEmpleado     = idEmpleado
        });

        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Planillas");

        worksheet.Cell(1, 1).Value = "Reporte de planillas";
        worksheet.Range(1, 1, 1, 15).Merge().Style
            .Font.SetBold()
            .Font.SetFontSize(16)
            .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center)
            .Alignment.SetVertical(XLAlignmentVerticalValues.Center);

        worksheet.Cell(2, 1).Value = $"Periodo: {periodo ?? "Todos"} | Departamento: {departamento ?? "Todos"}";
        worksheet.Cell(2, 1).Style.Font.SetItalic();
        worksheet.Range(2, 1, 2, 15).Merge();

        var headers = new[]
        {
            "Empleado", "Cargo", "Departamento", "Período", "Salario básico", "Comisiones",
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
            worksheet.Cell(currentRow, 3).Value = plan.Departamento;
            worksheet.Cell(currentRow, 4).Value = plan.PeriodoMesAnio;
            worksheet.Cell(currentRow, 5).Value = plan.SalarioBase;
            worksheet.Cell(currentRow, 6).Value = plan.Comisiones;
            worksheet.Cell(currentRow, 7).Value = plan.TotalHorasExtras;
            worksheet.Cell(currentRow, 8).Value = plan.PagoHorasExtras;
            worksheet.Cell(currentRow, 9).Value = plan.Incentivos;
            worksheet.Cell(currentRow, 10).Value = plan.TotalIngresos;
            worksheet.Cell(currentRow, 11).Value = plan.InssLaboral;
            worksheet.Cell(currentRow, 12).Value = plan.IrLaboral;
            worksheet.Cell(currentRow, 13).Value = plan.TotalDeducciones;
            worksheet.Cell(currentRow, 14).Value = plan.SalarioNeto;
            worksheet.Cell(currentRow, 15).Value = plan.FechaEmision;

            worksheet.Range(currentRow, 5, currentRow, 6).Style.NumberFormat.Format = "#,##0.00";
            worksheet.Range(currentRow, 8, currentRow, 15).Style.NumberFormat.Format = "#,##0.00";
            worksheet.Cell(currentRow, 15).Style.DateFormat.Format = "yyyy-mm-dd";
            worksheet.Range(currentRow, 1, currentRow, 15).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

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
        worksheet.Column(3).Width = 18;
        worksheet.Column(4).Width = 14;
        worksheet.Column(15).Width = 16;

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

