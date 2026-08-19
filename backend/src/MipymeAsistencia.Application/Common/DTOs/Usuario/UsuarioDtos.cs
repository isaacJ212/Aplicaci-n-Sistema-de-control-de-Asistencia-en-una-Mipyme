namespace MipymeAsistencia.Application.Common.DTOs.Usuario;

public class CambiarEstadoUsuarioDto
{
    public bool EstadoActivo { get; set; }
}

public class CambiarRolUsuarioDto
{
    public int IdRol { get; set; }
}

public class ResetPasswordUsuarioDto
{
    public string NuevaPassword { get; set; } = string.Empty;
}

public class RolDto
{
    public int IdRol { get; set; }
    public string NombreRol { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
}

public class CrearUsuarioRequestDto
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public int IdRol { get; set; } = 3; // Empleado por defecto
}
