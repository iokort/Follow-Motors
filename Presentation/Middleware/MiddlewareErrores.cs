namespace FollowMotors.Presentation.Middleware;

using FluentValidation;
using FollowMotors.Application.Common.Excepciones;
using Microsoft.AspNetCore.Mvc;
using MySqlConnector;

/// <summary>Traduce errores del dominio, validación y persistencia a HTTP controlado.</summary>
public sealed class ErroresMiddleware(RequestDelegate siguiente, ILogger<ErroresMiddleware> registro)
{
    /// <summary>Evita devolver trazas internas o información de conexión al consumidor.</summary>
    public async Task InvokeAsync(HttpContext contexto)
    {
        try { await siguiente(contexto); }
        catch (Exception ex)
        {
            if (contexto.Response.HasStarted) throw;
            registro.LogError(ex, "Error al procesar la solicitud {Ruta}", contexto.Request.Path);
            int estado = ex switch
            {
                ValidationException => StatusCodes.Status400BadRequest,
                CredencialesInvalidasException => StatusCodes.Status401Unauthorized,
                UnauthorizedAccessException => StatusCodes.Status403Forbidden,
                KeyNotFoundException => StatusCodes.Status404NotFound,
                ArgumentException => StatusCodes.Status400BadRequest,
                InvalidOperationException => StatusCodes.Status409Conflict,
                MySqlException e when e.Number == 1062 => StatusCodes.Status409Conflict,
                MySqlException e when e.Number == 1452 => StatusCodes.Status409Conflict,
                _ => StatusCodes.Status500InternalServerError
            };
            contexto.Response.StatusCode = estado;
            var detalle = estado == 500 ? "Ocurrió un error interno." :
                ex is MySqlException ? "El registro duplicado o la relación no es válida." : ex.Message;
            var problema = new ProblemDetails
            {
                Status = estado,
                Title = estado == 500 ? "Error interno" : "No se pudo completar la operación",
                Detail = detalle
            };
            if (ex is ValidationException validacion)
                problema.Extensions["errores"] = validacion.Errors
                    .Select(e => new { Campo = e.PropertyName, Mensaje = e.ErrorMessage }).ToArray();
            await contexto.Response.WriteAsJsonAsync(problema, contexto.RequestAborted);
        }
    }
}
