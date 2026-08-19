namespace MipymeAsistencia.Application.Common.DTOs.Planilla;

public class GenerarPlanillaPorDepartamentoRequestDto
{
    public string PeriodoMesAnio { get; set; } = string.Empty;
    public string? Departamento { get; set; } = "Todos";
    public decimal ComisionesGenerales { get; set; } = 0m;
    public decimal IncentivosGenerales { get; set; } = 0m;
    public decimal OtrasDeduccionesGenerales { get; set; } = 0m;
}

public class GenerarPlanillaPorDepartamentoResponseDto
{
    public string PeriodoMesAnio { get; set; } = string.Empty;
    public string Departamento { get; set; } = "Todos";
    public int TotalEmpleadosEncontrados { get; set; }
    public int TotalPlanillasGeneradas { get; set; }
    public int TotalPlanillasOmitidasPorExistir { get; set; }
    public decimal TotalMontoNetoGenerado { get; set; }
    public List<PlanillaResponseDto> Planillas { get; set; } = new();
    public string Mensaje { get; set; } = string.Empty;
}
