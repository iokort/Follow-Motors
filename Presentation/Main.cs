using System.Text;
using System.Security.Cryptography;
using FollowMotors.Infrastructure.Demo;
using FollowMotors.Application;
using FollowMotors.Application.Abstractions.Autenticacion;
using FollowMotors.Infrastructure;
using FollowMotors.Presentation.Autenticacion;
using FollowMotors.Presentation.Inicializacion;
using FollowMotors.Presentation.Middleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);
// La clave de demostración se genera cada vez que arranca el servidor; no se guarda en el ZIP.
var modoDemo = builder.Environment.IsDevelopment() && builder.Configuration.GetValue<bool>("Demo:Activado");
if (modoDemo)
    builder.Configuration["Jwt:Clave"] = Convert.ToBase64String(RandomNumberGenerator.GetBytes(48));
var secreto = builder.Configuration["Jwt:Clave"];
if (string.IsNullOrWhiteSpace(secreto) || Encoding.UTF8.GetByteCount(secreto) < 32)
    throw new InvalidOperationException("Configure Jwt:Clave con al menos 32 bytes por variable de entorno.");

builder.Services.AddControllers();
builder.Services.AddHttpContextAccessor();
builder.Services.AddApplication();
if (modoDemo)
    builder.Services.AddDemoInfrastructure();
else
    builder.Services.AddInfrastructure(builder.Configuration.GetConnectionString("MySql") ?? "");
builder.Services.AddCors(opciones => opciones.AddPolicy("AngularLocal", politica => politica
    .WithOrigins("http://localhost:4200", "http://127.0.0.1:4200")
    .AllowAnyHeader().AllowAnyMethod()));
builder.Services.AddScoped<IUsuarioActual, UsuarioActualHttp>();
builder.Services.AddSingleton<IGeneradorTokens, GeneradorTokensJwt>();
builder.Services.AddScoped<InicializadorGerente>();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opciones =>
    {
        opciones.MapInboundClaims = true;
        opciones.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Emisor"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audiencia"],
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secreto)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(30)
        };
    });
builder.Services.AddAuthorization();

var app = builder.Build();
app.UseMiddleware<ErroresMiddleware>();
app.UseCors("AngularLocal");
app.UseAuthentication();
app.UseMiddleware<CuentasActivasMiddleware>();
app.UseAuthorization();
app.MapControllers();
// Comprobación técnica de disponibilidad, sin mostrar información privada.
app.MapGet("/api/estado", () => Results.Ok(new { servicio = "FollowMotors",
    disponible = true, modo = modoDemo ? "demostracion" : "mysql" }));

// El gerente inicial se configura fuera del repositorio y nunca mediante un endpoint público.
if (!modoDemo)
{
    using var alcance = app.Services.CreateScope();
    await alcance.ServiceProvider.GetRequiredService<InicializadorGerente>()
        .CrearSiHaceFaltaAsync(CancellationToken.None);
}

app.Run();
