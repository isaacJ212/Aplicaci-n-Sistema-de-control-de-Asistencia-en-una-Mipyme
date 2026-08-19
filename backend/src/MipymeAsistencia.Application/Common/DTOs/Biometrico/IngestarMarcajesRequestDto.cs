namespace MipymeAsistencia.Application.Common.DTOs.Biometrico;

public class MarcajeBiometricoItemDto
{
    public string NumeroEnrollamiento { get; set; } = string.Empty; // Cédula, INSS o ID
    public DateTime FechaHora { get; set; }
    public int TipoMarcaje { get; set; } = 0; // 0 Entrada, 1 Salida, 2 IniAlm, 3 FinAlm
    public string TipoVerificacion { get; set; } = "Huella";
}

public class IngestarMarcajesRequestDto
{
    public int IdDispositivo { get; set; }
    public List<MarcajeBiometricoItemDto> Marcajes { get; set; } = new();
}
