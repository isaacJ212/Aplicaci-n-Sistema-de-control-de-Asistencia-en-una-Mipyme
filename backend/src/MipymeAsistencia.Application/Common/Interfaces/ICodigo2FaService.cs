namespace MipymeAsistencia.Application.Common.Interfaces;


public interface ICodigo2FaService
{

    void Guardar(string email, string codigoPlano, TimeSpan? duracion = null);
    string? ObtenerUltimo(string email);
    void Invalidar(string email);
}
