using MediatR;
using MipymeAsistencia.Application.Common.DTOs;
using MipymeAsistencia.Application.Common.DTOs.Auth;
using MipymeAsistencia.Application.Features.Auth.Commands.Login;
using MipymeAsistencia.Application.Features.Auth.Commands.Register;
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

    /// <summary>
    /// Autentica un usuario y devuelve el JWT junto con el refresh token.
    /// </summary>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<LoginResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
        {
            var error = ApiResponse<object>.BadRequest("Email y password son obligatorios.");
            return BadRequest(error);
        }

        try
        {
            var data = await _mediator.Send(new LoginCommand
            {
                Email    = request.Email,
                Password = request.Password
            });

            var response = ApiResponse<LoginResponseDto>.Ok(data, "Inicio de sesión exitoso.");
            return Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            var error = ApiResponse<object>.Unauthorized(ex.Message);
            return Unauthorized(error);
        }
        catch (Exception)
        {
            var error = ApiResponse<object>.InternalError();
            return StatusCode(StatusCodes.Status500InternalServerError, error);
        }
    }

    /// <summary>
    /// Registra un nuevo usuario y devuelve sus datos (sin información sensible).
    /// </summary>
    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<RegisterResponseDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
        {
            var error = ApiResponse<object>.BadRequest("Email y password son obligatorios.");
            return BadRequest(error);
        }

        try
        {
            var data = await _mediator.Send(new RegisterCommand
            {
                Email    = request.Email,
                Password = request.Password,
                Role     = request.Role
            });

            var response = ApiResponse<RegisterResponseDto>.Created(data, "Usuario registrado correctamente.");
            return StatusCode(StatusCodes.Status201Created, response);
        }
        catch (InvalidOperationException ex)
        {
            var error = ApiResponse<object>.Conflict(ex.Message);
            return Conflict(error);
        }
        catch (Exception)
        {
            var error = ApiResponse<object>.InternalError();
            return StatusCode(StatusCodes.Status500InternalServerError, error);
        }
    }
}
