using System.Security.Claims;
using System.Text;
using System.Threading.RateLimiting;
using FollowMotors.Application;
using FollowMotors.Domain;
using FollowMotors.Infrastructure;
using FollowMotors.Presentation.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.IdentityModel.Tokens;
using MySqlConnector;

var builder = WebApplication.CreateBuilder(args);

var cadena = builder.Configuration.GetConnectionString("MySql");
if (string.IsNullOrWhiteSpace(cadena))
    throw new InvalidOperationException("Configure ConnectionStrings__MySql mediante variables de entorno.");
var clave = builder.Configuration["Jwt:Clave"];
if (string.IsNullOrWhiteSpace(clave) || Encoding.UTF8.GetByteCount(clave) < 32)
    throw new InvalidOperationException("Configure Jwt__Clave con al menos 32 bytes mediante variables de entorno.");

builder.Services.AddSingleton(new MySqlDataSourceBuilder(cadena).Build());
builder.Services.AddScoped<MySqlStore>();
builder.Services.AddScoped<IUsuarioQueries>(sp => sp.GetRequiredService<MySqlStore>());
builder.Services.AddScoped<IClientesStore>(sp => sp.GetRequiredService<MySqlStore>());
builder.Services.AddScoped<IMotocicletasStore>(sp => sp.GetRequiredService<MySqlStore>());
builder.Services.AddScoped<ICitasStore>(sp => sp.GetRequiredService<MySqlStore>());
builder.Services.AddScoped<IServiciosStore>(sp => sp.GetRequiredService<MySqlStore>());
builder.Services.AddScoped<IReportesQueries>(sp => sp.GetRequiredService<MySqlStore>());
builder.Services.AddScoped<IAvisosStore>(sp => sp.GetRequiredService<MySqlStore>());
builder.Services.AddSingleton<JwtSecurityService>();
builder.Services.AddSingleton<IProtectorContrasenas>(sp => sp.GetRequiredService<JwtSecurityService>());
builder.Services.AddSingleton<IGeneradorJwt>(sp => sp.GetRequiredService<JwtSecurityService>());
builder.Services.AddScoped<LoginHandler>();
builder.Services.AddScoped<ClienteCommandHandler>();
builder.Services.AddScoped<MotocicletaCommandHandler>();
builder.Services.AddScoped<CitaCommandHandler>();
builder.Services.AddScoped<ServicioCommandHandler>();
builder.Services.AddCors(options => options.AddPolicy("frontend", p => p
    .WithOrigins(builder.Configuration["Cors:OrigenAngular"] ?? "http://localhost:4200")
    .AllowAnyHeader().AllowAnyMethod()));
builder.Services.AddRateLimiter(options => options.AddFixedWindowLimiter("login", limiter =>
{
    limiter.PermitLimit = 10;
    limiter.Window = TimeSpan.FromMinutes(1);
    limiter.QueueLimit = 0;
}));
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
{
    options.MapInboundClaims = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true, ValidIssuer = builder.Configuration["Jwt:Emisor"],
        ValidateAudience = true, ValidAudience = builder.Configuration["Jwt:Audiencia"],
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(clave)),
        ValidateLifetime = true, ClockSkew = TimeSpan.FromSeconds(30)
    };
});
builder.Services.AddAuthorization();

var app = builder.Build();
app.UseExceptionHandler(handler => handler.Run(async ctx =>
{
    var error = ctx.Features.Get<IExceptionHandlerPathFeature>()?.Error;
    var code = error switch
    {
        ArgumentException => StatusCodes.Status400BadRequest,
        UnauthorizedAccessException => StatusCodes.Status403Forbidden,
        InvalidOperationException => StatusCodes.Status409Conflict,
        _ => StatusCodes.Status500InternalServerError
    };
    ctx.Response.StatusCode = code;
    await Results.Problem(statusCode: code,
        title: code == 500 ? "Error interno del servidor" : error?.Message)
        .ExecuteAsync(ctx);
}));
app.UseCors("frontend");
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

static long UsuarioId(ClaimsPrincipal usuario) => long.Parse(usuario.FindFirstValue(ClaimTypes.NameIdentifier)!);
static bool ClienteFinal(ClaimsPrincipal usuario) => usuario.IsInRole(Roles.Cliente);
static async Task<long?> ClienteId(ClaimsPrincipal usuario, IClientesStore store, CancellationToken ct) =>
    ClienteFinal(usuario) ? await store.BuscarIdPorUsuarioAsync(UsuarioId(usuario), ct) : null;

app.MapGet("/api/estado", () => Results.Ok(new { servicio = "FollowMotors", modo = "mysql" }));
app.MapPost("/api/autenticacion/iniciar-sesion", async (IniciarSesionCommand cmd, LoginHandler handler, CancellationToken ct) =>
{
    var sesion = await handler.Handle(cmd, ct);
    return sesion is null ? Results.Unauthorized() : Results.Ok(sesion);
}).RequireRateLimiting("login");
app.MapGet("/api/autenticacion/mi-perfil", async (ClaimsPrincipal user, IUsuarioQueries queries, CancellationToken ct) =>
{
    if (!await queries.ActivoAsync(UsuarioId(user), ct)) return Results.Unauthorized();
    return Results.Ok(new { id = UsuarioId(user), rol = user.FindFirstValue(ClaimTypes.Role), identificador = user.Identity?.Name });
}).RequireAuthorization();

var clientes = app.MapGroup("/api/clientes").RequireAuthorization();
clientes.MapGet("", (IClientesStore s, CancellationToken ct) => s.ListarAsync(ct))
    .RequireAuthorization(p => p.RequireRole(Roles.Gerente, Roles.Empleado));
clientes.MapGet("/mi-expediente", async (ClaimsPrincipal u, IClientesStore s, CancellationToken ct) =>
{
    var id = await s.BuscarIdPorUsuarioAsync(UsuarioId(u), ct);
    return id is null ? Results.NotFound() : Results.Ok(await s.ObtenerAsync(id.Value, ct));
}).RequireAuthorization(p => p.RequireRole(Roles.Cliente));
clientes.MapGet("/{id:long}", async (long id, IClientesStore s, CancellationToken ct) =>
{
    var c = await s.ObtenerAsync(id, ct);
    return c is null ? Results.NotFound() : Results.Ok(c);
}).RequireAuthorization(p => p.RequireRole(Roles.Gerente, Roles.Empleado));
clientes.MapPost("", async (CrearClienteCommand cmd, ClaimsPrincipal u, ClienteCommandHandler h, CancellationToken ct) =>
    Results.Created("/api/clientes", new { id = await h.CrearAsync(cmd, UsuarioId(u), ct) }))
    .RequireAuthorization(p => p.RequireRole(Roles.Gerente, Roles.Empleado));
clientes.MapPut("/{id:long}", async (long id, ActualizarClienteCommand cmd, ClienteCommandHandler h, CancellationToken ct) =>
    await h.ActualizarAsync(id, cmd, ct) ? Results.NoContent() : Results.NotFound())
    .RequireAuthorization(p => p.RequireRole(Roles.Gerente, Roles.Empleado));

var motos = app.MapGroup("/api/motocicletas").RequireAuthorization();
motos.MapGet("", async (ClaimsPrincipal u, IMotocicletasStore s, IClientesStore c, CancellationToken ct) =>
{
    var owner = await ClienteId(u, c, ct);
    if (ClienteFinal(u) && owner is null) return Results.NotFound();
    return Results.Ok(await s.ListarAsync(owner, ct));
});
motos.MapPost("", async (RegistrarMotocicletaCommand cmd, MotocicletaCommandHandler h, CancellationToken ct) =>
    Results.Created("/api/motocicletas", new { id = await h.RegistrarAsync(cmd, ct) }))
    .RequireAuthorization(p => p.RequireRole(Roles.Gerente, Roles.Empleado));
motos.MapPut("/{id:long}", async (long id, ActualizarMotocicletaCommand cmd, IMotocicletasStore s, CancellationToken ct) =>
{
    if (id <= 0 || string.IsNullOrWhiteSpace(cmd.Marca) || string.IsNullOrWhiteSpace(cmd.Modelo) ||
        cmd.KilometrajeActual < 0 || cmd.Anio is < 1900 or > 2150) return Results.BadRequest();
    return await s.ActualizarAsync(id, cmd, ct) ? Results.NoContent() : Results.NotFound();
}).RequireAuthorization(p => p.RequireRole(Roles.Gerente, Roles.Empleado));

var citas = app.MapGroup("/api/citas").RequireAuthorization();
citas.MapGet("", async (ClaimsPrincipal u, ICitasStore s, IClientesStore c, CancellationToken ct) =>
{
    var owner = await ClienteId(u, c, ct);
    if (ClienteFinal(u) && owner is null) return Results.NotFound();
    return Results.Ok(await s.ListarAsync(owner, ct));
});
citas.MapPost("", async (ReservarCitaCommand cmd, ClaimsPrincipal u, IClientesStore c,
    CitaCommandHandler h, CancellationToken ct) =>
{
    var owner = await ClienteId(u, c, ct);
    if (ClienteFinal(u) && (owner is null || owner != cmd.ClienteId)) return Results.Forbid();
    var id = await h.ReservarAsync(cmd, ClienteFinal(u) ? "WEB" : "PRESENCIAL", ct);
    return Results.Created($"/api/citas/{id}", new { id });
});
citas.MapPut("/{id:long}", async (long id, ReprogramarCitaCommand cmd, ClaimsPrincipal u, IClientesStore c,
    ICitasStore s, CancellationToken ct) =>
{
    var owner = await ClienteId(u, c, ct);
    if (ClienteFinal(u) && owner is null) return Results.Forbid();
    return await s.ReprogramarAsync(id, cmd, owner, ct) ? Results.NoContent() : Results.NotFound();
});
citas.MapDelete("/{id:long}", async (long id, ClaimsPrincipal u, IClientesStore c, ICitasStore s, CancellationToken ct) =>
{
    var owner = await ClienteId(u, c, ct);
    if (ClienteFinal(u) && owner is null) return Results.Forbid();
    return await s.CancelarAsync(id, owner, ct) ? Results.NoContent() : Results.NotFound();
});

var servicios = app.MapGroup("/api/servicios").RequireAuthorization();
servicios.MapGet("", async (ClaimsPrincipal u, IServiciosStore s, IClientesStore c, CancellationToken ct) =>
{
    var owner = await ClienteId(u, c, ct);
    if (ClienteFinal(u) && owner is null) return Results.NotFound();
    return Results.Ok(await s.ListarAsync(owner, ct));
});
servicios.MapPost("", async (RegistrarAtencionCommand cmd, ServicioCommandHandler h, CancellationToken ct) =>
    Results.Created("/api/servicios", new { id = await h.RegistrarAsync(cmd, ct) }))
    .RequireAuthorization(p => p.RequireRole(Roles.Gerente, Roles.Empleado));
servicios.MapPut("/{id:long}/validar", async (long id, ValidarServicioCommand cmd, IServiciosStore s, CancellationToken ct) =>
    await s.ValidarAsync(id, cmd, ct) ? Results.NoContent() : Results.NotFound())
    .RequireAuthorization(p => p.RequireRole(Roles.Gerente));

app.MapGet("/api/garantias", async (ClaimsPrincipal u, IServiciosStore s, IClientesStore c, CancellationToken ct) =>
{
    var owner = await ClienteId(u, c, ct);
    if (ClienteFinal(u) && owner is null) return Results.NotFound();
    return Results.Ok(await s.GarantiasAsync(owner, ct));
}).RequireAuthorization();
app.MapGet("/api/reportes/indicadores", (IReportesQueries q, CancellationToken ct) =>
    q.ConsultarIndicadoresAsync(ct)).RequireAuthorization(p => p.RequireRole(Roles.Gerente));
app.MapGet("/api/notificaciones", async (ClaimsPrincipal u, long? clienteId, IAvisosStore s,
    IClientesStore c, CancellationToken ct) =>
{
    var owner = await ClienteId(u, c, ct);
    if (ClienteFinal(u) && owner is null) return Results.Forbid();
    var target = owner ?? clienteId;
    return target is null ? Results.BadRequest("Indica el cliente.") :
        Results.Ok(await s.ListarAsync(target.Value, ct));
}).RequireAuthorization();
app.MapPost("/api/notificaciones", async (CrearAvisoCommand cmd, IAvisosStore s, CancellationToken ct) =>
    Results.Created("/api/notificaciones", new { id = await s.GenerarAsync(cmd, ct) }))
    .RequireAuthorization(p => p.RequireRole(Roles.Gerente, Roles.Empleado));

app.Run();
