using FollowMotors.Api.Infrastructure;
using MySqlConnector;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddProblemDetails();
builder.Services.AddSingleton<IDatabaseConnectionFactory, MySqlConnectionFactory>();
builder.Services.AddCors(options =>
{
    options.AddPolicy("frontend", policy =>
    {
        var origins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
        if (origins.Length > 0)
        {
            policy.WithOrigins(origins).AllowAnyHeader().AllowAnyMethod();
        }
    });
});

var app = builder.Build();

app.UseExceptionHandler();
app.UseCors("frontend");

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
