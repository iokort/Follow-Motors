using Microsoft.AspNetCore.Identity;
using FollowMotors.Api.Infrastructure;

namespace FollowMotors.Api.Application.Auth;

/// <summary>
/// Solicitud de inicio de sesión para el personal existente. No hay endpoint de
/// autorregistro: Gael definió que el gerente entrega credenciales en sucursal.
/// </summary>
public sealed record StaffLoginRequest(string Correo, string Contrasena);

/// <summary>Identidad validada y permisos obtenidos de las tablas de Gael.</summary>
/// <summary>
/// Cuenta validada desde MySQL: roles y permisos vienen de sus tablas, no de Angular.
/// </summary>
public sealed record StaffIdentity(
    Guid Id, string Nombre, IReadOnlyList<string> Permisos, IReadOnlyList<string> Roles);

/// <summary>
/// Puerto de autenticación; el controlador no conoce MySQL ni los hashes.
/// Se usa PasswordHasher de ASP.NET Core (formato Identity PBKDF2).
/// </summary>
public interface IStaffAuthService
{
    Task<StaffIdentity?> LoginAsync(StaffLoginRequest request, CancellationToken ct);
}

/// <summary>
/// Valida cuentas YA CREADAS en MySQL V3; no inventa contraseñas ni las almacena.
/// Las cuentas deben tener hash compatible con ASP.NET Core PasswordHasher.
/// </summary>
public sealed class MySqlStaffAuthService(IDatabaseConnectionFactory factory) : IStaffAuthService
{
    private sealed record StaffAccount(Guid Id, string Nombre, string Hash);

    private readonly PasswordHasher<StaffAccount> _hasher = new();

    public async Task<StaffIdentity?> LoginAsync(StaffLoginRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Correo) ||
            string.IsNullOrEmpty(request.Contrasena) ||
            request.Correo.Length > 254 || request.Contrasena.Length > 500)
            return null;

        await using var connection = factory.Create();
        await connection.OpenAsync(ct);

        // Se verifica usuario ACTIVO, sin revelar si el correo existe o no.
        await using var userSql = new MySqlConnector.MySqlCommand("""
            SELECT id, nombre, hash_password
            FROM usuarios WHERE correo = @Correo AND activo = 1 LIMIT 1
            """, connection);
        userSql.Parameters.AddWithValue("@Correo", request.Correo.Trim());
        StaffAccount? account;
        await using (var reader = await userSql.ExecuteReaderAsync(ct))
        {
            account = await reader.ReadAsync(ct)
                ? new StaffAccount(reader.GetGuid(0), reader.GetString(1), reader.GetString(2))
                : null;
        }
        if (account is null) return null;

        // PasswordHasher nunca compara contraseñas en texto plano ni SHA casero.
        var status = _hasher.VerifyHashedPassword(account, account.Hash, request.Contrasena);
        if (status == PasswordVerificationResult.Failed) return null;

        // Autorización respaldada por roles_permisos, no por un campo editable
        // recibido del navegador. Mismo usuario puede tener varios roles.
        await using var rolesSql = new MySqlConnector.MySqlCommand("""
            SELECT DISTINCT r.codigo, rp.permiso_codigo
            FROM usuarios_roles ur
            JOIN roles r ON r.codigo = ur.rol_codigo AND r.activo = 1
            JOIN roles_permisos rp ON rp.rol_codigo = ur.rol_codigo
            WHERE ur.usuario_id = @Id
              AND rp.permiso_codigo IN ('agenda.ver', 'agenda.crear', 'agenda.modificar')
            """, connection);
        rolesSql.Parameters.AddWithValue("@Id", account.Id.ToString());

        // US01: obtener roles ACTIVOS y permisos directamente de MySQL V3.
        // Un usuario con agenda.ver no se convierte por eso en gerente.
        var roles = new HashSet<string>(StringComparer.Ordinal);
        var perms = new HashSet<string>(StringComparer.Ordinal);
        await using (var reader = await rolesSql.ExecuteReaderAsync(ct))
        {
            while (await reader.ReadAsync(ct))
            {
                roles.Add(reader.GetString(0));
                perms.Add(reader.GetString(1));
            }
        }

        // Solo personal con permisos obtiene sesión en esta ruta.
        if (perms.Count == 0) return null;
        return new StaffIdentity(account.Id, account.Nombre, perms.ToArray(), roles.ToArray());
    }
}
