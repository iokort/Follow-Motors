namespace FollowMotors.Presentation.Autenticacion;

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using FollowMotors.Application.Abstractions.Autenticacion;
using Microsoft.IdentityModel.Tokens;

/// <summary>Emite credenciales JWT firmadas con una clave obtenida de la configuración segura.</summary>
public sealed class GeneradorTokensJwt(IConfiguration configuracion) : IGeneradorTokens
{
    /// <summary>Entrega un token de dos horas con identidad y rol que verificará la API.</summary>
    public string Generar(ulong usuarioId, string identificador, string rol)
    {
        var clave = configuracion["Jwt:Clave"]
            ?? throw new InvalidOperationException("No existe clave JWT configurada.");
        var credenciales = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(clave)), SecurityAlgorithms.HmacSha256);
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, usuarioId.ToString()),
            new Claim(ClaimTypes.Name, identificador),
            new Claim(ClaimTypes.Role, rol),
        };
        var token = new JwtSecurityToken(
            issuer: configuracion["Jwt:Emisor"],
            audience: configuracion["Jwt:Audiencia"],
            claims: claims, expires: DateTime.UtcNow.AddHours(2),
            signingCredentials: credenciales);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
