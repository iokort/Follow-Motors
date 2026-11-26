namespace FollowMotors.Infrastructure.Autenticacion;

using System.Security.Cryptography;
using FollowMotors.Application.Abstractions.Autenticacion;

/// <summary>Protege contraseñas mediante PBKDF2-SHA256 y sal aleatoria por usuario.</summary>
public sealed class ProtectorContrasenas : IProtectorContrasenas
{
    private const int Iteraciones = 310_000;
    private const int TamanoSal = 16;
    private const int TamanoHash = 32;

    /// <summary>Genera un hash con sal distinta y lo serializa sin almacenar la contraseña.</summary>
    public string GenerarHash(string contrasena)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(contrasena);
        byte[] sal = RandomNumberGenerator.GetBytes(TamanoSal);
        byte[] hash = Rfc2898DeriveBytes.Pbkdf2(contrasena, sal, Iteraciones,
            HashAlgorithmName.SHA256, TamanoHash);
        return $"pbkdf2-sha256${Iteraciones}${Convert.ToBase64String(sal)}${Convert.ToBase64String(hash)}";
    }

    /// <summary>Verifica el hash con comparación de tiempo constante.</summary>
    public bool Verificar(string contrasena, string hash)
    {
        if (string.IsNullOrEmpty(contrasena) || string.IsNullOrWhiteSpace(hash)) return false;
        try
        {
            var partes = hash.Split('$');
            if (partes.Length != 4 || partes[0] != "pbkdf2-sha256" ||
                !int.TryParse(partes[1], out int iteraciones) || iteraciones < 100_000 ||
                iteraciones > 1_000_000) return false;
            byte[] sal = Convert.FromBase64String(partes[2]);
            byte[] esperado = Convert.FromBase64String(partes[3]);
            if (sal.Length != TamanoSal || esperado.Length != TamanoHash) return false;
            byte[] actual = Rfc2898DeriveBytes.Pbkdf2(contrasena, sal, iteraciones,
                HashAlgorithmName.SHA256, TamanoHash);
            return CryptographicOperations.FixedTimeEquals(actual, esperado);
        }
        catch (FormatException) { return false; }
    }
}
