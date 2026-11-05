// PSEUDOCÓDIGO | INICIO: importar el espacio de nombres requerido por esta instrucción.
using Microsoft.AspNetCore.Identity;
// PSEUDOCÓDIGO | INICIO: importar el espacio de nombres requerido por esta instrucción.
using FollowMotors.Api.Infrastructure;

// PSEUDOCÓDIGO | INICIO: ubicar las clases en su módulo de autenticación de Follow Motors.
namespace FollowMotors.Api.Application.Auth;

/// <summary>
/// Solicitud de inicio de sesión para el personal existente. No hay endpoint de
/// autorregistro: Gael definió que el gerente entrega credenciales en sucursal.
/// </summary>
// PSEUDOCÓDIGO | DATOS: correo y contraseña proporcionados por el usuario en el formulario.
public sealed record StaffLoginRequest(string Correo, string Contrasena);

/// <summary>
/// Cuenta validada desde MySQL: roles y permisos vienen de sus tablas, no de Angular.
/// </summary>
// PSEUDOCÓDIGO | DATOS: construir la identidad que ya pasó las validaciones del servidor.
public sealed record StaffIdentity(
    // PSEUDOCÓDIGO | DATOS: almacenar ID, nombre, permisos y roles obtenidos de MySQL.
    Guid Id, string Nombre, IReadOnlyList<string> Permisos, IReadOnlyList<string> Roles);

/// <summary>
/// Puerto de autenticación; el controlador no conoce MySQL ni los hashes.
/// Se usa PasswordHasher de ASP.NET Core (formato Identity PBKDF2).
/// </summary>
// PSEUDOCÓDIGO | INICIO CONTRATO: definir la operación de login sin depender del proveedor MySQL.
public interface IStaffAuthService
// PSEUDOCÓDIGO | INICIO BLOQUE: ejecutar las instrucciones de esta estructura.
{
    // PSEUDOCÓDIGO | PROCESO: recibir credenciales y devolver identidad válida o NULO.
    Task<StaffIdentity?> LoginAsync(StaffLoginRequest request, CancellationToken ct);
// PSEUDOCÓDIGO | FIN BLOQUE: terminar la estructura actual.
}

/// <summary>
/// Valida cuentas YA CREADAS en MySQL V3; no inventa contraseñas ni las almacena.
/// Las cuentas deben tener hash compatible con ASP.NET Core PasswordHasher.
/// </summary>
// PSEUDOCÓDIGO | INICIO CLASE: autenticación de personal contra la base de datos MySQL V3.
public sealed class MySqlStaffAuthService(IDatabaseConnectionFactory factory) : IStaffAuthService
// PSEUDOCÓDIGO | INICIO BLOQUE: ejecutar las instrucciones de esta estructura.
{
    // PSEUDOCÓDIGO | DATOS: representar temporalmente ID, nombre y hash leídos de MySQL.
    private sealed record StaffAccount(Guid Id, string Nombre, string Hash);

    // PSEUDOCÓDIGO | DECLARAR: preparar el verificador de contraseña PBKDF2 de ASP.NET Core.
    private readonly PasswordHasher<StaffAccount> _hasher = new();

    // PSEUDOCÓDIGO | INICIO LOGIN: verificar credenciales y consultar roles de la cuenta.
    public async Task<StaffIdentity?> LoginAsync(StaffLoginRequest request, CancellationToken ct)
    // PSEUDOCÓDIGO | INICIO BLOQUE: ejecutar las instrucciones de esta estructura.
    {
        // PSEUDOCÓDIGO | COMPARAR SI: el correo está vacío o contiene únicamente espacios.
        if (string.IsNullOrWhiteSpace(request.Correo) ||
            // PSEUDOCÓDIGO | O SI: el usuario no proporcionó una contraseña.
            string.IsNullOrEmpty(request.Contrasena) ||
            // PSEUDOCÓDIGO | O SI: el correo o la contraseña superan los límites permitidos.
            request.Correo.Length > 254 || request.Contrasena.Length > 500)
            // PSEUDOCÓDIGO | SI:SÍ -> FIN: rechazar el acceso sin revelar qué dato fue incorrecto.
            return null;

        // PSEUDOCÓDIGO | ABRIR RECURSO: crear conexión a MySQL; cerrarla automáticamente al terminar.
        await using var connection = factory.Create();
        // PSEUDOCÓDIGO | PROCESO: abrir la conexión real con la base de datos.
        await connection.OpenAsync(ct);

        // Se verifica usuario ACTIVO, sin revelar si el correo existe o no.
        // PSEUDOCÓDIGO | CONSULTAR: buscar en usuarios una cuenta activa por su correo, usando parámetro SQL.
        await using var userSql = new MySqlConnector.MySqlCommand("""
            SELECT id, nombre, hash_password
            FROM usuarios WHERE correo = @Correo AND activo = 1 LIMIT 1
            """, connection);
        // PSEUDOCÓDIGO | DATO: enviar el correo como parámetro; no concatenarlo dentro del SQL.
        userSql.Parameters.AddWithValue("@Correo", request.Correo.Trim());
        // PSEUDOCÓDIGO | DATO: declarar usuario encontrado; al inicio puede ser NULO.
        StaffAccount? account;
        // PSEUDOCÓDIGO | LEER: ejecutar búsqueda de usuario y liberar después el lector SQL.
        await using (var reader = await userSql.ExecuteReaderAsync(ct))
        // PSEUDOCÓDIGO | INICIO BLOQUE: ejecutar las instrucciones de esta estructura.
        {
            // PSEUDOCÓDIGO | COMPARAR SI: la consulta devolvió por lo menos un usuario.
            account = await reader.ReadAsync(ct)
                // PSEUDOCÓDIGO | SI:SÍ -> DATOS: cargar ID, nombre y hash desde el resultado SQL.
                ? new StaffAccount(reader.GetGuid(0), reader.GetString(1), reader.GetString(2))
                // PSEUDOCÓDIGO | SI:NO -> DATO: dejar la cuenta vacía porque no se encontró usuario.
                : null;
        // PSEUDOCÓDIGO | FIN BLOQUE: terminar la estructura actual.
        }
        // PSEUDOCÓDIGO | COMPARAR SI: ninguna cuenta activa coincide con ese correo.
        if (account is null) return null;

        // PasswordHasher nunca compara contraseñas en texto plano ni SHA casero.
        // PSEUDOCÓDIGO | PROCESO: verificar contraseña contra el hash almacenado sin descifrarlo.
        var status = _hasher.VerifyHashedPassword(account, account.Hash, request.Contrasena);
        // PSEUDOCÓDIGO | COMPARAR SI: la verificación criptográfica de la contraseña falló.
        if (status == PasswordVerificationResult.Failed) return null;

        // Autorización respaldada por roles_permisos, no por un campo editable
        // recibido del navegador. Mismo usuario puede tener varios roles.
        // PSEUDOCÓDIGO | CONSULTAR: buscar roles activos y permisos asociados al usuario identificado.
        await using var rolesSql = new MySqlConnector.MySqlCommand("""
            SELECT DISTINCT r.codigo, rp.permiso_codigo
            FROM usuarios_roles ur
            JOIN roles r ON r.codigo = ur.rol_codigo AND r.activo = 1
            JOIN roles_permisos rp ON rp.rol_codigo = ur.rol_codigo
            WHERE ur.usuario_id = @Id
              AND rp.permiso_codigo IN ('agenda.ver', 'agenda.crear', 'agenda.modificar')
            """, connection);
        // PSEUDOCÓDIGO | DATO: enviar el identificador de usuario como parámetro SQL.
        rolesSql.Parameters.AddWithValue("@Id", account.Id.ToString());

        // US01: obtener roles ACTIVOS y permisos directamente de MySQL V3.
        // Un usuario con agenda.ver no se convierte por eso en gerente.
        // PSEUDOCÓDIGO | DATOS: inicializar conjunto sin duplicados para los roles activos.
        var roles = new HashSet<string>(StringComparer.Ordinal);
        // PSEUDOCÓDIGO | DATOS: inicializar conjunto sin duplicados para los permisos.
        var perms = new HashSet<string>(StringComparer.Ordinal);
        // PSEUDOCÓDIGO | LEER: ejecutar búsqueda de roles y permisos del usuario.
        await using (var reader = await rolesSql.ExecuteReaderAsync(ct))
        // PSEUDOCÓDIGO | INICIO BLOQUE: ejecutar las instrucciones de esta estructura.
        {
            // PSEUDOCÓDIGO | MIENTRAS existan filas SQL: recorrer un rol y su permiso asociado.
            while (await reader.ReadAsync(ct))
            // PSEUDOCÓDIGO | INICIO BLOQUE: ejecutar las instrucciones de esta estructura.
            {
                // PSEUDOCÓDIGO | PROCESO: añadir el código del rol leído de la primera columna.
                roles.Add(reader.GetString(0));
                // PSEUDOCÓDIGO | PROCESO: añadir el permiso leído de la segunda columna.
                perms.Add(reader.GetString(1));
            // PSEUDOCÓDIGO | FIN BLOQUE: terminar la estructura actual.
            }
        // PSEUDOCÓDIGO | FIN BLOQUE: terminar la estructura actual.
        }

        // Solo personal con permisos obtiene sesión en esta ruta.
        // PSEUDOCÓDIGO | COMPARAR SI: el usuario no tiene permisos válidos para esta sesión.
        if (perms.Count == 0) return null;
        // PSEUDOCÓDIGO | FIN LOGIN -> REGRESAR: ID, nombre, permisos y roles confiables.
        return new StaffIdentity(account.Id, account.Nombre, perms.ToArray(), roles.ToArray());
    // PSEUDOCÓDIGO | FIN BLOQUE: terminar la estructura actual.
    }
// PSEUDOCÓDIGO | FIN BLOQUE: terminar la estructura actual.
}
