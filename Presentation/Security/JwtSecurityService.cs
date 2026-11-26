using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using FollowMotors.Application;
using FollowMotors.Domain;
using Microsoft.IdentityModel.Tokens;

namespace FollowMotors.Presentation.Security;

public sealed class JwtSecurityService(IConfiguration config) : IProtectorContrasenas, IGeneradorJwt
{
    private const int Iteraciones = 310_000;
    public string Generar(string contrasena)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(contrasena);
        var sal = RandomNumberGenerator.GetBytes(16);
        var hash = Rfc2898DeriveBytes.Pbkdf2(contrasena, sal, Iteraciones, HashAlgorithmName.SHA256, 32);
        return $"pbkdf2-sha256${Iteraciones}${Convert.ToBase64String(sal)}${Convert.ToBase64String(hash)}";
    }
    public bool Verificar(string contrasena, string hash)
    {
        if (string.IsNullOrEmpty(contrasena) || string.IsNullOrEmpty(hash)) return false;
        try
        {
            var partes = hash.Split('$');
            if (partes.Length != 4 || partes[0] != "pbkdf2-sha256" ||
                !int.TryParse(partes[1], out var iteraciones) ||
                iteraciones is < 100_000 or > 1_000_000) return false;
            var sal = Convert.FromBase64String(partes[2]);
            var esperado = Convert.FromBase64String(partes[3]);
            if (sal.Length != 16 || esperado.Length != 32) return false;
            var calculado = Rfc2898DeriveBytes.Pbkdf2(contrasena, sal, iteraciones, HashAlgorithmName.SHA256, 32);
            return CryptographicOperations.FixedTimeEquals(calculado, esperado);
        }
        catch (FormatException) { return false; }
    }
    public SesionResponse Crear(Usuario usuario)
    {
        var secreto = config["Jwt:Clave"] ?? throw new InvalidOperationException("Falta Jwt:Clave.");
        var expires = DateTime.UtcNow.AddMinutes(30);
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
            new Claim(ClaimTypes.Name, usuario.Identificador),
            new Claim(ClaimTypes.Role, usuario.Rol),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N"))
        };
        var token = new JwtSecurityToken(
            issuer: config["Jwt:Emisor"],
            audience: config["Jwt:Audiencia"],
            claims: claims,
            expires: expires,
            signingCredentials: new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secreto)), SecurityAlgorithms.HmacSha256));
        return new SesionResponse(new JwtSecurityTokenHandler().WriteToken(token), usuario.Rol, usuario.Id, expires);
    }
}
