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
builder.Services.AddSingleton<IDatabaseConnectionFactory, MySqlConnectionFactory>();
builder.Services.AddScoped<IAgendaRepository, MySqlAgendaRepository>();
builder.Services.AddScoped<IAgendaService, AgendaService>();
builder.Services.AddScoped<AgendaPolicy>();
builder.Services.AddScoped<IStaffAuthService, MySqlStaffAuthService>();

// ================================================================
// Autenticación con cookies firmadas por ASP.NET Core.
// - No hay usuarios de ejemplo ni contraseñas incrustadas en código.
// - Requiere una cuenta vigente de MySQL V3 con hash Identity PBKDF2.
// - SameSite Strict, HttpOnly y Secure en producción.
// - 401/403 en API en vez de redirección HTML a formulario.
// ================================================================
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "FollowMotors.StaffSession";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Strict;
        options.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
            ? CookieSecurePolicy.SameAsRequest
            : CookieSecurePolicy.Always;
        options.ExpireTimeSpan = TimeSpan.FromHours(6);
        options.SlidingExpiration = false;
        options.Events.OnRedirectToLogin = ctx =>
        {
            ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return Task.CompletedTask;
        };
        options.Events.OnRedirectToAccessDenied = ctx =>
        {
            ctx.Response.StatusCode = StatusCodes.Status403Forbidden;
            return Task.CompletedTask;
        };
    });

// La autorización se decide con permisos originados en roles_permisos de Gael.
// No confiar en los botones ocultos ni en roles enviados por el navegador.
builder.Services.AddAuthorization(options =>
{
    // US01: el servidor valida que el rol GERENTE exista en MySQL V3.
    options.AddPolicy("Gerencia", p => p.RequireAuthenticatedUser().RequireRole("GERENTE"));
    options.AddPolicy("AgendaVer", p => p.RequireAuthenticatedUser().RequireClaim("permission", "agenda.ver"));
    options.AddPolicy("AgendaCrear", p => p.RequireAuthenticatedUser().RequireClaim("permission", "agenda.crear"));
    options.AddPolicy("AgendaModificar", p => p.RequireAuthenticatedUser().RequireClaim("permission", "agenda.modificar"));
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
app.UseAuthentication();
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
