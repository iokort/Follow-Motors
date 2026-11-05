using FollowMotors.Api.Application.Agenda;
using FollowMotors.Api.Application.Auth;
using FollowMotors.Api.Infrastructure;
using FollowMotors.Api.Infrastructure.Agenda;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using MySqlConnector;

internal static class IntegrationSpecs
{
    /// <summary>
    /// Prueba optativa contra MySQL 8.4 REAL. Solo se activa cuando CI entrega
    /// FOLLOW_MOTORS_TEST_MYSQL_ROOT; jamás tocar una DB productiva o compartida.
    /// Crea únicamente la base follow_motors_v3_dev del entorno efímero de CI.
    /// </summary>
    public static async Task RunIfConfiguredAsync()
    {
        var root = Environment.GetEnvironmentVariable("FOLLOW_MOTORS_TEST_MYSQL_ROOT");
        if (string.IsNullOrWhiteSpace(root))
        {
            Console.WriteLine("Integración MySQL V3: omitida (no configurada).");
            return;
        }

        // El test necesita poder crear tablas y usuarios en MySQL efímero.
        // Ningún correo ni contraseña de un usuario real se introduce en scripts.
        var builder = new MySqlConnectionStringBuilder(root) { Database = "" };
        await using var admin = new MySqlConnection(builder.ConnectionString);
        await admin.OpenAsync();

        foreach (var number in new[] { "00", "01", "02", "03", "04", "05" })
        {
            var directory = Path.Combine(Directory.GetCurrentDirectory(), "database", "v3", "sql");
            var file = Directory.GetFiles(directory, $"{number}_*.sql").Single();
            await using var command = new MySqlCommand(await File.ReadAllTextAsync(file), admin);
            await command.ExecuteNonQueryAsync();
            Console.WriteLine($"V3 SQL cargado: {Path.GetFileName(file)}");
        }

        // La app usa la DB nueva, no follow_motors del ejemplo técnico V2.
        var appString = new MySqlConnectionStringBuilder(root)
        {
            Database = "follow_motors_v3_dev"
        }.ConnectionString;

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Default"] = appString
            }).Build();
        var factory = new MySqlConnectionFactory(config);
        var repository = new MySqlAgendaRepository(factory);
        var agenda = new AgendaService(repository, new AgendaPolicy(TimeProvider.System));
        var login = new MySqlStaffAuthService(factory);
        var usuario = Guid.NewGuid();
        var correo = $"ci-agenda-{usuario:N}@example.invalid";
        var password = Guid.NewGuid().ToString("N") + "$-solo-CI";

        // Este hash tiene el mismo formato ASP.NET Core Identity que comprueba
        // MySqlStaffAuthService. Se crea solo en la base efímera de pruebas.
        var hash = new PasswordHasher<object>().HashPassword(new object(), password);
        await using (var conn = factory.Create())
        {
            await conn.OpenAsync();
            await using var cmd = new MySqlCommand("""
                INSERT INTO usuarios(id, nombre, correo, hash_password)
                VALUES (@Id, 'Gerente de integración', @Correo, @Hash);
                INSERT INTO usuarios_roles(usuario_id, rol_codigo) VALUES (@Id, 'GERENTE');
                """, conn);
            cmd.Parameters.AddWithValue("@Id", usuario.ToString());
            cmd.Parameters.AddWithValue("@Correo", correo);
            cmd.Parameters.AddWithValue("@Hash", hash);
            await cmd.ExecuteNonQueryAsync();
        }

        var identidad = await login.LoginAsync(new StaffLoginRequest(correo, password), default);
        Check(identidad?.Id == usuario, "La sesión usa el ID del usuario real de V3.");
        Check(identidad!.Permisos.Contains("agenda.crear"), "La autorización lee agenda.crear desde roles_permisos.");
        Check(identidad.Permisos.Contains("agenda.modificar"), "La autorización lee agenda.modificar desde roles_permisos.");
        Check(await login.LoginAsync(new StaffLoginRequest(correo, "Incorrecta"), default) is null,
            "Contraseña inválida no genera sesión.");

        var zona = TimeZoneInfo.FindSystemTimeZoneById("America/Mexico_City");
        var inicio = TimeZoneInfo.ConvertTime(TimeProvider.System.GetUtcNow(), zona).DateTime
            .Date.AddDays(3).AddHours(11);

        // US06: cita de otra marca sin registro de motocicleta del portal.
        var cita = await agenda.CrearAsync(
            new CrearCitaRequest("EXTERNA", null, "Honda", "CB125", "Cliente de prueba",
                "0000000000", inicio, 60, "Diagnóstico de prueba"), usuario, default);
        Check(cita.TipoMotocicleta == "EXTERNA" && cita.MotocicletaId is null,
            "US06: cita externa real persistida en MySQL V3.");

        // US15: listar y consultar una cita de la base.
        var lista = await agenda.ListarDiaAsync(DateOnly.FromDateTime(inicio), default);
        Check(lista.Any(x => x.Id == cita.Id), "US15: consulta de agenda real.");
        Check((await agenda.BuscarAsync(cita.Id, default))?.Id == cita.Id,
            "US15: detalle de cita real.");

        // US15: actualización y cancelación sin borrar el registro.
        var cambio = await agenda.ReprogramarAsync(cita.Id,
            new ReprogramarCitaRequest(inicio.AddHours(1), 90), usuario, default);
        Check(cambio?.DuracionMinutos == 90, "US15: reprogramación real.");
        var cancelada = await agenda.CancelarAsync(cita.Id,
            new CancelarCitaRequest("Cliente canceló por WhatsApp"), usuario, default);
        Check(cancelada?.Estado == "CANCELADA", "US15: cancelación real.");
        Check((await agenda.BuscarAsync(cita.Id, default))?.Estado == "CANCELADA",
            "US15: la cita no se elimina físicamente.");

        // US06: también aceptar una CFMOTO previamente vinculada al esquema V3.
        var cliente = Guid.NewGuid();
        var moto = Guid.NewGuid();
        await using (var conn = factory.Create())
        {
            await conn.OpenAsync();
            await using var cmd = new MySqlCommand("""
                INSERT INTO clientes(id, nombre, registrado_por)
                VALUES (@Cliente, 'Cliente ficticio', @Actor);
                INSERT INTO motocicletas
                  (id, cliente_actual_id, plan_id, vin, marca, modelo,
                   fecha_venta, origen, registrada_por)
                SELECT @Moto, @Cliente, id, @Vin, 'CFMOTO', '450 SR',
                       '2026-01-10', 'FOLLOW_MOTORS', @Actor
                FROM planes_mantenimiento WHERE codigo = 'CFMOTO_2026_V1';
                """, conn);
            cmd.Parameters.AddWithValue("@Cliente", cliente.ToString());
            cmd.Parameters.AddWithValue("@Actor", usuario.ToString());
            cmd.Parameters.AddWithValue("@Moto", moto.ToString());
            cmd.Parameters.AddWithValue("@Vin", "VIN" + Guid.NewGuid().ToString("N")[..14]);
            await cmd.ExecuteNonQueryAsync();
        }

        var citaCfMoto = await agenda.CrearAsync(
            new CrearCitaRequest("CFMOTO", moto, null, null, null, null, inicio, 60, "Primer servicio"),
            usuario, default);
        Check(citaCfMoto.MotocicletaId == moto, "US06: cita CFMOTO vinculada a moto real V3.");

        // Auditoría real: una creación, cambio y cancelación deben dejar 3 registros.
        await using (var conn = factory.Create())
        {
            await conn.OpenAsync();
            await using var cmd = new MySqlCommand("""
                SELECT COUNT(*) FROM auditoria
                WHERE entidad = 'citas' AND entidad_id = @Cita AND actor_id = @Actor
                """, conn);
            cmd.Parameters.AddWithValue("@Cita", cita.Id.ToString());
            cmd.Parameters.AddWithValue("@Actor", usuario.ToString());
            var auditCount = Convert.ToInt32(await cmd.ExecuteScalarAsync());
            Check(auditCount == 3, "US06/US15: auditoría transaccional (crear/cambiar/cancelar).");
        }

        Console.WriteLine("MySQL V3 real: autenticación, agenda y auditoría correctas.");
    }

    private static void Check(bool condition, string message)
    {
        if (!condition) throw new Exception("Integración fallida: " + message);
        Console.WriteLine("OK MySQL: " + message);
    }
}
