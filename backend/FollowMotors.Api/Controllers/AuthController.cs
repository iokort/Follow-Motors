// PSEUDOCÓDIGO | INICIO: importar el espacio de nombres requerido por esta instrucción.
using System.Security.Claims;
// PSEUDOCÓDIGO | INICIO: importar el espacio de nombres requerido por esta instrucción.
using FollowMotors.Api.Application.Auth;
// PSEUDOCÓDIGO | INICIO: importar el espacio de nombres requerido por esta instrucción.
using Microsoft.AspNetCore.Authentication;
// PSEUDOCÓDIGO | INICIO: importar el espacio de nombres requerido por esta instrucción.
using Microsoft.AspNetCore.Authentication.Cookies;
// PSEUDOCÓDIGO | INICIO: importar el espacio de nombres requerido por esta instrucción.
using Microsoft.AspNetCore.Authorization;
// PSEUDOCÓDIGO | INICIO: importar el espacio de nombres requerido por esta instrucción.
using Microsoft.AspNetCore.Mvc;

// PSEUDOCÓDIGO | INICIO: ubicar las clases en su módulo de autenticación de Follow Motors.
namespace FollowMotors.Api.Controllers;

/// <summary>
/// US01: autenticación REAL del gerente/asesor desde el esquema MySQL V3.
/// Se reutilizan las mismas rutas de sesión para US06 y US15.
/// </summary>
// PSEUDOCÓDIGO | DECLARAR: este controlador recibe solicitudes HTTP de la API.
[ApiController]
// PSEUDOCÓDIGO | DATO: establecer la ruta común de los servicios de autenticación.
[Route("api/v1/auth")]
// PSEUDOCÓDIGO | INICIO CONTROLADOR: recibir el servicio de login y el registro de eventos.
public sealed class AuthController(IStaffAuthService authService, ILogger<AuthController> logger) : ControllerBase
// PSEUDOCÓDIGO | INICIO BLOQUE: ejecutar las instrucciones de esta estructura.
{
    /// <summary>
    /// Sesión de personal con cookie HttpOnly, SameSite Strict y expiración;
    /// solo autentica cuentas existentes en el esquema de Gael.
    /// </summary>
    // PSEUDOCÓDIGO | DECLARAR: aceptar una petición POST en la ruta indicada.
    [HttpPost("staff-login")]
    // PSEUDOCÓDIGO | DATO: recibir la información de entrada en formato JSON.
    [Consumes("application/json")]
    // PSEUDOCÓDIGO | PERMITIR: el ingreso de credenciales antes de que exista sesión.
    [AllowAnonymous]
    // PSEUDOCÓDIGO | INICIO LOGIN HTTP: recibir JSON del navegador y consultar al servicio.
    public async Task<IActionResult> Login([FromBody] StaffLoginRequest request, CancellationToken ct)
    // PSEUDOCÓDIGO | INICIO BLOQUE: ejecutar las instrucciones de esta estructura.
    {
        // PSEUDOCÓDIGO | PROCESO: validar credenciales y consultar usuario usando servicio C#.
        var user = await authService.LoginAsync(request, ct);
        // PSEUDOCÓDIGO | COMPARAR SI: el servicio negó el acceso o no encontró cuenta válida.
        if (user is null)
        // PSEUDOCÓDIGO | INICIO BLOQUE: ejecutar las instrucciones de esta estructura.
        {
            // CRC-US01-12: no registrar el correo ni el secreto en la bitácora.
            // PSEUDOCÓDIGO | WRITE: registrar intento denegado sin revelar correo ni contraseña.
            logger.LogWarning("US01: intento de inicio de sesión rechazado");
            // PSEUDOCÓDIGO | SI:SÍ -> REGRESAR: respuesta HTTP 401 sin conceder sesión.
            return Unauthorized(new ProblemDetails
            // PSEUDOCÓDIGO | INICIO BLOQUE: ejecutar las instrucciones de esta estructura.
            {
                // PSEUDOCÓDIGO | DATO: definir el título genérico que se mostrará al usuario.
                Title = "No se pudo iniciar sesión",
                // PSEUDOCÓDIGO | DATO: explicar el rechazo sin indicar si existe el correo.
                Detail = "Credenciales inválidas o cuenta sin permisos.",
                // PSEUDOCÓDIGO | DATO: establecer el código 401 de autenticación fallida.
                Status = StatusCodes.Status401Unauthorized
            // PSEUDOCÓDIGO | FIN DATOS/PROCESO: cerrar la construcción de datos o la llamada.
            });
        // PSEUDOCÓDIGO | FIN BLOQUE: terminar la estructura actual.
        }

        // PSEUDOCÓDIGO | DATOS: crear lista de características firmadas de la sesión.
        var claims = new List<Claim>
        // PSEUDOCÓDIGO | INICIO BLOQUE: ejecutar las instrucciones de esta estructura.
        {
            // PSEUDOCÓDIGO | DATO: agregar el ID real del usuario verificado.
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            // PSEUDOCÓDIGO | DATO: agregar el nombre real del usuario verificado.
            new(ClaimTypes.Name, user.Nombre)
        // PSEUDOCÓDIGO | FIN DATOS/PROCESO: cerrar la construcción de datos o la llamada.
        };
        // PSEUDOCÓDIGO | PARA CADA permiso: añadirlo como claim de autorización.
        claims.AddRange(user.Permisos.Select(p => new Claim("permission", p)));
        // Rol obtenido de usuarios_roles y roles activos; no del navegador.
        // PSEUDOCÓDIGO | PARA CADA rol de MySQL: añadirlo como claim de rol firmado.
        claims.AddRange(user.Roles.Select(r => new Claim(ClaimTypes.Role, r)));

        // PSEUDOCÓDIGO | PROCESO: iniciar sesión del usuario en ASP.NET Core.
        await HttpContext.SignInAsync(
            // PSEUDOCÓDIGO | DATO: utilizar el esquema de cookies configurado en el servidor.
            CookieAuthenticationDefaults.AuthenticationScheme,
            // PSEUDOCÓDIGO | DATO: construir identidad autenticada con sus claims.
            new ClaimsPrincipal(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme)),
            // PSEUDOCÓDIGO | DATOS: definir características y vencimiento de la sesión.
            new AuthenticationProperties
            // PSEUDOCÓDIGO | INICIO BLOQUE: ejecutar las instrucciones de esta estructura.
            {
                // PSEUDOCÓDIGO | DATO: establecer sesión que no se conserva permanentemente.
                IsPersistent = false,
                // PSEUDOCÓDIGO | DATO: fijar fecha de vencimiento a seis horas desde el ingreso.
                ExpiresUtc = DateTimeOffset.UtcNow.AddHours(6)
            // PSEUDOCÓDIGO | FIN DATOS/PROCESO: cerrar la construcción de datos o la llamada.
            });

        // PSEUDOCÓDIGO | WRITE: registrar un acceso aceptado sin incluir secretos.
        logger.LogInformation("US01: sesión de personal creada");
        // Nunca devolver contraseña, hash ni secretos en el JSON.
        // PSEUDOCÓDIGO | FIN LOGIN HTTP -> WRITE: devolver nombre, roles y permisos; nunca el hash.
        return Ok(new { usuario = user.Nombre, roles = user.Roles, permisos = user.Permisos });
    // PSEUDOCÓDIGO | FIN BLOQUE: terminar la estructura actual.
    }

    /// <summary>Eliminar la sesión al terminar el trabajo en sucursal.</summary>
    // PSEUDOCÓDIGO | DECLARAR: aceptar una petición POST en la ruta indicada.
    [HttpPost("logout")]
    // PSEUDOCÓDIGO | COMPARAR SI: el usuario tiene una sesión vigente.
    [Authorize]
    // PSEUDOCÓDIGO | INICIO LOGOUT: recibir petición para terminar la sesión.
    public async Task<IActionResult> Logout()
    // PSEUDOCÓDIGO | INICIO BLOQUE: ejecutar las instrucciones de esta estructura.
    {
        // PSEUDOCÓDIGO | PROCESO: eliminar la cookie de autenticación del usuario.
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        // PSEUDOCÓDIGO | FIN LOGOUT -> REGRESAR HTTP 204: sesión cerrada correctamente.
        return NoContent();
    // PSEUDOCÓDIGO | FIN BLOQUE: terminar la estructura actual.
    }

    /// <summary>
    /// US01: comprueba el permiso de entrar al área de GERENCIA.
    /// Devuelve 401 sin sesión, 403 para cuentas con otro rol y 200 al gerente.
    /// </summary>
    // PSEUDOCÓDIGO | DECLARAR: aceptar una petición GET en la ruta indicada.
    [HttpGet("gerencia/acceso")]
    // PSEUDOCÓDIGO | COMPARAR SI: existe sesión y el usuario cumple la política GERENTE.
    [Authorize(Policy = "Gerencia")]
    // PSEUDOCÓDIGO | INICIO VALIDACIÓN GERENTE: la política de arriba ya verificó el rol.
    public IActionResult VerificarAccesoGerencia() =>
        // PSEUDOCÓDIGO | FIN VALIDACIÓN -> REGRESAR: el acceso gerencial fue autorizado.
        Ok(new { acceso = "GERENTE", usuario = User.Identity?.Name });

    /// <summary>Consulta mínima de identidad para un frontend Angular.</summary>
    // PSEUDOCÓDIGO | DECLARAR: aceptar una petición GET en la ruta indicada.
    [HttpGet("me")]
    // PSEUDOCÓDIGO | COMPARAR SI: el usuario tiene una sesión vigente.
    [Authorize]
    // PSEUDOCÓDIGO | INICIO CONSULTAR SESIÓN: responder con información mínima del usuario.
    public IActionResult Me() => Ok(new
    // PSEUDOCÓDIGO | INICIO BLOQUE: ejecutar las instrucciones de esta estructura.
    {
        // PSEUDOCÓDIGO | DATO: obtener el nombre de la identidad autenticada.
        usuario = User.Identity?.Name,
        // PSEUDOCÓDIGO | DATOS: listar roles confiables incluidos en la cookie.
        roles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToArray(),
        // PSEUDOCÓDIGO | DATOS: listar permisos confiables incluidos en la cookie.
        permisos = User.FindAll("permission").Select(c => c.Value).ToArray()
    // PSEUDOCÓDIGO | FIN DATOS/PROCESO: cerrar la construcción de datos o la llamada.
    });
// PSEUDOCÓDIGO | FIN BLOQUE: terminar la estructura actual.
}
