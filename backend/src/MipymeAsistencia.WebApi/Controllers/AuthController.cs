using System.Security.Claims;
using MediatR;
using MipymeAsistencia.Application.Common.DTOs;
using MipymeAsistencia.Application.Common.DTOs.Auth;
using MipymeAsistencia.Application.Features.Auth.Commands.Login;
using MipymeAsistencia.Application.Features.Auth.Commands.Logout;
using MipymeAsistencia.Application.Features.Auth.Commands.RefreshToken;
using MipymeAsistencia.Application.Features.Auth.Commands.Register;
using MipymeAsistencia.Application.Features.Auth.Commands.Verify2Fa;
using MipymeAsistencia.Application.Features.Auth.Commands.Enable2Fa;
using MipymeAsistencia.Application.Features.Auth.Queries.GetCurrentUser;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MipymeAsistencia.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>Autentica un usuario y devuelve JWT + refresh token.</summary>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<LoginResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
    {
        var data = await _mediator.Send(new LoginCommand
        {
            Email    = request.Email,
            Password = request.Password
        });

        return Ok(ApiResponse<LoginResponseDto>.Ok(data, "Inicio de sesión exitoso."));
    }

    /// <summary>Registra un nuevo usuario y devuelve sus datos sin información sensible.</summary>
    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<RegisterResponseDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
    {
        var data = await _mediator.Send(new RegisterCommand
        {
            Email    = request.Email,
            Password = request.Password,
            Role     = request.Role
        });

        return StatusCode(
            StatusCodes.Status201Created,
            ApiResponse<RegisterResponseDto>.Created(data, "Usuario registrado correctamente."));
    }

    /// <summary>Renueva el JWT usando un refresh token válido (rotación de tokens).</summary>
    [HttpPost("refresh")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<LoginResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequestDto request)
    {
        var data = await _mediator.Send(new RefreshTokenCommand
        {
            RefreshToken = request.RefreshToken
        });

        return Ok(ApiResponse<LoginResponseDto>.Ok(data, "Token renovado correctamente."));
    }

    /// <summary>Verifica el código de seguridad de dos pasos y emite el JWT si es válido.</summary>
    [HttpPost("verify-2fa")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<LoginResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Verify2Fa([FromBody] Verify2FaRequestDto request)
    {
        var data = await _mediator.Send(new Verify2FaCommand
        {
            Email = request.Email,
            Code = request.Code
        });

        return Ok(ApiResponse<LoginResponseDto>.Ok(data, "Validación de dos pasos exitosa."));
    }

    /// <summary>Activa o desactiva la autenticación en dos pasos del usuario actual.</summary>
    [HttpPost("toggle-2fa")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Toggle2Fa([FromBody] Enable2FaRequestDto request)
    {
        var email = User.FindFirstValue(ClaimTypes.Email) ?? User.FindFirstValue("sub");
        if (string.IsNullOrWhiteSpace(email))
            return Unauthorized(ApiResponse<object>.Unauthorized("Token inválido."));

        var data = await _mediator.Send(new Enable2FaCommand
        {
            Email = email,
            Enabled = request.Enabled
        });

        return Ok(ApiResponse<object>.Ok(data, request.Enabled ? "Autenticación en dos pasos activada." : "Autenticación en dos pasos desactivada."));
    }

    /// <summary>Devuelve los datos del usuario autenticado extraídos del JWT.</summary>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<CurrentUserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Me()
    {
        // El email viene del claim del JWT validado por el middleware de autenticación
        var email = User.FindFirstValue(ClaimTypes.Email)
                    ?? User.FindFirstValue("sub");

        if (string.IsNullOrWhiteSpace(email))
            return Unauthorized(ApiResponse<object>.Unauthorized("Token inválido."));

        var data = await _mediator.Send(new GetCurrentUserQuery { Email = email });

        return Ok(ApiResponse<CurrentUserDto>.Ok(data, "Usuario autenticado."));
    }

    /// <summary>Revoca el refresh token para cerrar sesión del dispositivo actual.</summary>
    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Logout([FromBody] LogoutRequestDto request)
    {
        await _mediator.Send(new LogoutCommand { RefreshToken = request.RefreshToken });

        return Ok(ApiResponse<object>.Ok(null!, "Sesión cerrada correctamente."));
    }
}
