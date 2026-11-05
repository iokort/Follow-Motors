using FollowMotors.Api.Application.Agenda;
using FollowMotors.Api.Application.Auth;
using FollowMotors.Api.Infrastructure;
using FollowMotors.Api.Infrastructure.Agenda;
using Microsoft.AspNetCore.Authentication.Cookies;
using MySqlConnector;

var builder = WebApplication.CreateBuilder(args);

// ================================================================
// Dependencias (D de SOLID)
// El backend solo conoce contratos. El repositorio MySQL implementa
// persistencia, AgendaService aplica reglas y Controllers traducen HTTP.
// ================================================================
builder.Services.AddProblemDetails();
builder.Services.AddControllers();
builder.Services.AddSingleton(TimeProvider.System);
 // US01 | Conexión de usuarios mediante la fábrica MySQL ya compartida.
builder.Services.AddSingleton<IDatabaseConnectionFactory, MySqlConnectionFactory>();
builder.Services.AddScoped<IAgendaRepository, MySqlAgendaRepository>();
builder.Services.AddScoped<IAgendaService, AgendaService>();
builder.Services.AddScoped<AgendaPolicy>();
// PSEUDOCÓDIGO | DECLARAR: asociar el contrato de login con el servicio que consulta MySQL real.
builder.Services.AddScoped<IStaffAuthService, MySqlStaffAuthService>();

// ================================================================
// Autenticación con cookies firmadas por ASP.NET Core.
// - No hay usuarios de ejemplo ni contraseñas incrustadas en código.
// - Requiere una cuenta vigente de MySQL V3 con hash Identity PBKDF2.
// - SameSite Strict, HttpOnly y Secure en producción.
// - 401/403 en API en vez de redirección HTML a formulario.
// ================================================================
// PSEUDOCÓDIGO | INICIO: activar la autenticación mediante cookies firmadas por ASP.NET Core.
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    // PSEUDOCÓDIGO | DATOS: establecer las reglas de sesión en el servidor.
    .AddCookie(options =>
    // PSEUDOCÓDIGO | INICIO BLOQUE: ejecutar las instrucciones del conjunto.
    {
        // PSEUDOCÓDIGO | DATO: asignar un nombre identificable a la cookie del sistema.
        options.Cookie.Name = "FollowMotors.StaffSession";
        // PSEUDOCÓDIGO | DATO: impedir que JavaScript lea la cookie del navegador.
        options.Cookie.HttpOnly = true;
        // PSEUDOCÓDIGO | DATO: restringir el envío de la cookie a solicitudes del mismo sitio.
        options.Cookie.SameSite = SameSiteMode.Strict;
        // PSEUDOCÓDIGO | COMPARAR SI: estamos ejecutando en desarrollo o producción.
        options.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
            // PSEUDOCÓDIGO | SI:SÍ -> permitir HTTP local según el protocolo de la solicitud.
            ? CookieSecurePolicy.SameAsRequest
            // PSEUDOCÓDIGO | SI:NO -> exigir transmisión de la cookie por HTTPS.
            : CookieSecurePolicy.Always;
        // PSEUDOCÓDIGO | DATO: establecer seis horas como duración máxima de la sesión.
        options.ExpireTimeSpan = TimeSpan.FromHours(6);
        // PSEUDOCÓDIGO | DATO: evitar extender la sesión por actividad del usuario.
        options.SlidingExpiration = false;
        // PSEUDOCÓDIGO | COMPARAR SI: una petición accede sin haberse autenticado.
        options.Events.OnRedirectToLogin = ctx =>
        // PSEUDOCÓDIGO | INICIO BLOQUE: ejecutar las instrucciones del conjunto.
        {
            // PSEUDOCÓDIGO | SI:SÍ -> REGRESAR HTTP 401 cuando faltan credenciales válidas.
            ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
            // PSEUDOCÓDIGO | FIN PROCESO: cerrar el evento HTTP sin redirección de página.
            return Task.CompletedTask;
        // PSEUDOCÓDIGO | FIN PROCESO: terminar la configuración del objeto.
        };
        // PSEUDOCÓDIGO | COMPARAR SI: usuario autenticado carece del permiso requerido.
        options.Events.OnRedirectToAccessDenied = ctx =>
        // PSEUDOCÓDIGO | INICIO BLOQUE: ejecutar las instrucciones del conjunto.
        {
            // PSEUDOCÓDIGO | SI:SÍ -> REGRESAR HTTP 403 cuando no tiene autorización.
            ctx.Response.StatusCode = StatusCodes.Status403Forbidden;
            // PSEUDOCÓDIGO | FIN PROCESO: cerrar el evento HTTP sin redirección de página.
            return Task.CompletedTask;
        // PSEUDOCÓDIGO | FIN PROCESO: terminar la configuración del objeto.
        };
    // PSEUDOCÓDIGO | FIN BLOQUE: terminar la estructura.
    });

// La autorización se decide con permisos originados en roles_permisos de Gael.
// No confiar en los botones ocultos ni en roles enviados por el navegador.
// PSEUDOCÓDIGO | INICIO: declarar políticas de autorización del backend.
builder.Services.AddAuthorization(options =>
// PSEUDOCÓDIGO | INICIO BLOQUE: ejecutar las instrucciones del conjunto.
{
    // US01: el servidor valida que el rol GERENTE exista en MySQL V3.
    // PSEUDOCÓDIGO | COMPARAR SI: la sesión tiene el rol GERENTE obtenido de la base real.
    options.AddPolicy("Gerencia", p => p.RequireAuthenticatedUser().RequireRole("GERENTE"));
    // PSEUDOCÓDIGO | COMPARAR SI: la sesión tiene permiso de consultar la agenda.
    options.AddPolicy("AgendaVer", p => p.RequireAuthenticatedUser().RequireClaim("permission", "agenda.ver"));
    // PSEUDOCÓDIGO | COMPARAR SI: la sesión tiene permiso de registrar citas.
    options.AddPolicy("AgendaCrear", p => p.RequireAuthenticatedUser().RequireClaim("permission", "agenda.crear"));
    // PSEUDOCÓDIGO | COMPARAR SI: la sesión tiene permiso de modificar citas.
    options.AddPolicy("AgendaModificar", p => p.RequireAuthenticatedUser().RequireClaim("permission", "agenda.modificar"));
// PSEUDOCÓDIGO | FIN BLOQUE: terminar la estructura.
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("frontend", policy =>
    {
        var origins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
        if (origins.Length > 0)
        {
            // No se habilitan credenciales cross-origin. Angular usa proxy /api
            // con mismo origen; las cookies Strict no deben enviarse a dominios ajenos.
            policy.WithOrigins(origins).AllowAnyHeader().AllowAnyMethod();
        }
    });
});

var app = builder.Build();
app.UseExceptionHandler();
app.UseCors("frontend");
// PSEUDOCÓDIGO | PROCESO: validar cookie de sesión antes de procesar endpoints.
app.UseAuthentication();
// PSEUDOCÓDIGO | PROCESO: aplicar políticas de rol y permisos antes de responder.
app.UseAuthorization();

// Registra los Controllers dedicados a agenda sin alterar los endpoints de salud.
app.MapControllers();

var api = app.MapGroup("/api/v1");
api.MapGet("/health", () => Results.Ok(new
{
    status = "ok",
    service = "follow-motors-api"
}))
.WithName("GetHealth")
.WithDescription("Verifica que la API está disponible; no consulta MySQL.");

if (app.Environment.IsDevelopment())
{
    api.MapGet("/health/database", async (
        IDatabaseConnectionFactory factory, CancellationToken cancellationToken) =>
    {
        try
        {
            await using var connection = factory.Create();
            await connection.OpenAsync(cancellationToken);
            return Results.Ok(new { status = "ok", database = "mysql" });
        }
        catch (Exception exception) when (exception is MySqlException or InvalidOperationException)
        {
            return Results.Problem(
                title: "Base de datos no disponible",
                detail: "Revisa la configuración de conexión de desarrollo.",
                statusCode: StatusCodes.Status503ServiceUnavailable);
        }
    })
    .WithName("GetDatabaseHealth")
    .WithDescription("Comprueba MySQL. Disponible únicamente en Development.");
}

app.Run();
public partial class Program { }
