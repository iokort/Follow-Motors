namespace FollowMotors.Infrastructure.Demo;

using System.Text.Json;
using FollowMotors.Application.Abstractions.Autenticacion;
using FollowMotors.Application.Abstractions.Demostracion;
using FollowMotors.Application.Abstractions.Repositories.Atenciones;
using FollowMotors.Application.Abstractions.Repositories.Auditoria;
using FollowMotors.Application.Abstractions.Repositories.Clientes;
using FollowMotors.Application.Abstractions.Repositories.Motocicletas;
using FollowMotors.Application.Abstractions.Repositories.Personal;
using FollowMotors.Application.Models.Demostracion;
using FollowMotors.Domain.Entities.Atenciones;
using FollowMotors.Domain.Entities.Auditoria;
using FollowMotors.Domain.Entities.Clientes;
using FollowMotors.Domain.Entities.Motocicletas;
using FollowMotors.Domain.Entities.Personal;
using FollowMotors.Infrastructure.Autenticacion;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Base de datos ficticia de FollowMotors. TODOS los datos de demostración y las
/// implementaciones temporales de los repositorios se concentran en este único archivo.
/// No se conecta a MySQL y se reinicia con cada ejecución de la API.
/// </summary>
public sealed class DatosEnMemoria : IRepositorioCuentas, IRepositorioClientes,
    IRepositorioPersonal, IRepositorioMotocicletas, IRepositorioAtenciones,
    IRepositorioAuditoria, IConsultaDemostracion
{
    private readonly object _candado = new();
    private readonly AccesoDemostracion[] _accesos =
    [
        new("gerente@follow.motors", "Gerente2026!", "GERENTE", "Andrea Morales"),
        new("asesor@follow.motors", "Asesor2026!!", "EMPLEADO", "Diego Salazar"),
        new("mecanico@follow.motors", "Mecanico2026!", "EMPLEADO", "Luis Navarro"),
        new("cliente@follow.motors", "Cliente2026!", "CLIENTE", "Mariana Torres")
    ];
    private readonly IProtectorContrasenas _protector;
    private readonly Dictionary<ulong, CuentaCliente> _cuentas = new();
    private readonly Dictionary<ulong, Cliente> _clientes = new();
    private readonly Dictionary<ulong, Empleado> _personal = new();
    private readonly Dictionary<ulong, Motocicleta> _motocicletas = new();
    private readonly Dictionary<ulong, AtencionTaller> _atenciones = new();
    private readonly Dictionary<ulong, CobroDemostracion> _cobros = new();
    private readonly List<RegistroAuditoria> _auditoria = new();
    private ulong _usuarioSiguiente = 1;
    private ulong _clienteSiguiente = 1;
    private ulong _empleadoSiguiente = 1;
    private ulong _motocicletaSiguiente = 1;
    private ulong _atencionSiguiente = 1;
    private ulong _cobroSiguiente = 1;
    private ulong _auditoriaSiguiente = 1;

    /// <summary>Tarifa fija ficticia, reemplazable por la tabla MySQL tarifas_revision.</summary>
    private const decimal TarifaFicticia = 350m;

    /// <summary>Inicializa cuentas ficticias y un expediente de ejemplo con hashes reales.</summary>
    public DatosEnMemoria(IProtectorContrasenas protector)
    {
        _protector = protector;
        ulong gerente = AgregarCuenta(_accesos[0].Identificador, _accesos[0].Contrasena, _accesos[0].Rol);
        ulong empleado = AgregarCuenta(_accesos[1].Identificador, _accesos[1].Contrasena, _accesos[1].Rol);
        ulong mecanico = AgregarCuenta(_accesos[2].Identificador, _accesos[2].Contrasena, _accesos[2].Rol);
        ulong cliente = AgregarCuenta(_accesos[3].Identificador, _accesos[3].Contrasena, _accesos[3].Rol);
        _personal[_empleadoSiguiente] = new Gerente(_empleadoSiguiente++, gerente, "Andrea", "Morales", true);
        _personal[_empleadoSiguiente] = new Empleado(_empleadoSiguiente++, empleado, "Diego", "Salazar", false, true);
        ulong mecanicoId = _empleadoSiguiente;
        _personal[_empleadoSiguiente] = new Mecanico(_empleadoSiguiente++, mecanico, "Luis", "Navarro", true);
        ulong clienteId = _clienteSiguiente;
        _clientes[_clienteSiguiente] = new Cliente(_clienteSiguiente++, cliente, "Mariana", "Torres",
            "4271002000", gerente, DateTime.UtcNow.AddDays(-4));
        ulong motoId = _motocicletaSiguiente;
        _motocicletas[_motocicletaSiguiente] = new Motocicleta(_motocicletaSiguiente++, clienteId,
            "DEMO-VIN-001", "QRO123A", "CFMOTO", "250 NK", 2024, "Negro", 4300,
            DateTime.UtcNow.AddDays(-3));
        _atenciones[_atencionSiguiente] = new AtencionTaller(_atencionSiguiente++, clienteId,
            motoId, null, mecanicoId, "Revisión general de muestra", 4300,
            null, "RECIBIDA", DateTime.UtcNow.AddDays(-1), null);
        RegistrarCambio(gerente, "sistema", 1, "INICIALIZACION_DEMO", null,
            new { descripcion = "Datos ficticios para exposición académica" });
    }

    /// <summary>Crea una cuenta con identificador único en la colección temporal.</summary>
    private ulong AgregarCuenta(string identificador, string contrasena, string rol)
    {
        ulong id = _usuarioSiguiente++;
        _cuentas[id] = new CuentaCliente(id, identificador.Trim().ToLowerInvariant(),
            _protector.GenerarHash(contrasena), rol, true, DateTime.UtcNow);
        return id;
    }

    /// <summary>Guarda un cambio sin exponer contraseñas ni hashes en los registros.</summary>
    private void RegistrarCambio(ulong? usuarioId, string entidad, ulong registroId,
        string accion, object? antes, object? despues)
    {
        _auditoria.Add(new RegistroAuditoria(_auditoriaSiguiente++, usuarioId, entidad,
            registroId, accion, antes is null ? null : JsonSerializer.Serialize(antes),
            despues is null ? null : JsonSerializer.Serialize(despues), DateTime.UtcNow));
    }

    /// <summary>Impide utilizar un identificador repetido para cuentas del mismo sistema.</summary>
    private void ValidarIdentificador(string identificador)
    {
        if (_cuentas.Values.Any(x => string.Equals(x.Identificador, identificador,
            StringComparison.OrdinalIgnoreCase)))
            throw new InvalidOperationException("Ya existe una cuenta con ese identificador.");
    }

    /// <summary>Localiza credenciales por el identificador interno del taller.</summary>
    public Task<CuentaCliente?> BuscarPorIdentificadorAsync(string identificador, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        lock (_candado) return Task.FromResult(_cuentas.Values.FirstOrDefault(
            x => string.Equals(x.Identificador, identificador, StringComparison.OrdinalIgnoreCase)));
    }

    /// <summary>Busca una cuenta sin depender del motor de persistencia.</summary>
    Task<CuentaCliente?> IRepositorioCuentas.ObtenerPorIdAsync(ulong usuarioId, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        lock (_candado) return Task.FromResult(_cuentas.GetValueOrDefault(usuarioId));
    }

    /// <summary>Comprueba si ya existe la cuenta gerencial inicial.</summary>
    public Task<bool> ExisteGerenteAsync(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        lock (_candado) return Task.FromResult(_cuentas.Values.Any(
            x => x.Rol == "GERENTE" && x.Activo));
    }

    /// <summary>Permite el arranque inicial sin duplicar gerentes cuando se cambia a MySQL.</summary>
    public Task<bool> CrearGerenteInicialAsync(string identificador, string hashContrasena,
        string nombres, string apellidos, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        lock (_candado)
        {
            if (_cuentas.Values.Any(x => x.Rol == "GERENTE" && x.Activo))
                return Task.FromResult(false);
            ValidarIdentificador(identificador);
            ulong usuarioId = _usuarioSiguiente++;
            _cuentas[usuarioId] = new CuentaCliente(usuarioId, identificador,
                hashContrasena, "GERENTE", true, DateTime.UtcNow);
            ulong id = _empleadoSiguiente++;
            _personal[id] = new Gerente(id, usuarioId, nombres, apellidos, true);
            RegistrarCambio(usuarioId, "usuarios", usuarioId, "ALTA_GERENTE_INICIAL", null,
                new { rol = "GERENTE" });
            return Task.FromResult(true);
        }
    }

    /// <summary>Registra cliente y cuenta en una sola sección crítica simulando transacción.</summary>
    public Task<ulong> CrearAsync(CuentaCliente cuenta, Cliente cliente,
        ulong usuarioActorId, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        lock (_candado)
        {
            ValidarIdentificador(cuenta.Identificador);
            ulong usuarioId = _usuarioSiguiente++;
            _cuentas[usuarioId] = cuenta with { Id = usuarioId, Rol = "CLIENTE", Activo = true };
            ulong clienteId = _clienteSiguiente++;
            _clientes[clienteId] = cliente with { Id = clienteId, UsuarioId = usuarioId,
                RegistradoPorUsuarioId = usuarioActorId };
            RegistrarCambio(usuarioActorId, "clientes", clienteId, "ALTA_CLIENTE", null,
                new { clienteId, usuarioId });
            return Task.FromResult(clienteId);
        }
    }

    /// <summary>Desactiva únicamente la cuenta y preserva los expedientes asociados.</summary>
    public Task<bool> DesactivarAsync(ulong clienteId, ulong usuarioActorId, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        lock (_candado)
        {
            if (!_clientes.TryGetValue(clienteId, out var cliente)
                || !_cuentas.TryGetValue(cliente.UsuarioId, out var cuenta)
                || !cuenta.Activo) return Task.FromResult(false);
            _cuentas[cuenta.Id] = cuenta with { Activo = false };
            RegistrarCambio(usuarioActorId, "clientes", clienteId, "DESACTIVAR_CUENTA",
                new { activo = true }, new { activo = false });
            return Task.FromResult(true);
        }
    }

    /// <summary>Obtiene únicamente propietarios con una cuenta todavía activa.</summary>
    Task<Cliente?> IRepositorioClientes.ObtenerPorUsuarioAsync(ulong usuarioId, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        lock (_candado) return Task.FromResult(_cuentas.TryGetValue(usuarioId, out var cuenta)
            && cuenta.Activo ? _clientes.Values.FirstOrDefault(x => x.UsuarioId == usuarioId) : null);
    }

    /// <summary>Obtiene el expediente histórico incluso si la cuenta fue desactivada.</summary>
    Task<Cliente?> IRepositorioClientes.ObtenerPorIdAsync(ulong id, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        lock (_candado) return Task.FromResult(_clientes.GetValueOrDefault(id));
    }

    /// <summary>Da de alta al personal con un rol operativo y audita la modificación.</summary>
    public Task<ulong> RegistrarAsync(string identificador, string hashContrasena,
        string nombres, string apellidos, bool esMecanico, ulong usuarioActorId, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        lock (_candado)
        {
            ValidarIdentificador(identificador);
            ulong usuarioId = _usuarioSiguiente++;
            _cuentas[usuarioId] = new CuentaCliente(usuarioId,
                identificador, hashContrasena, "EMPLEADO", true, DateTime.UtcNow);
            ulong empleadoId = _empleadoSiguiente++;
            _personal[empleadoId] = esMecanico
                ? new Mecanico(empleadoId, usuarioId, nombres, apellidos, true)
                : new Empleado(empleadoId, usuarioId, nombres, apellidos, false, true);
            RegistrarCambio(usuarioActorId, "empleados", empleadoId, "ALTA_PERSONAL", null,
                new { empleadoId, esMecanico });
            return Task.FromResult(empleadoId);
        }
    }

    /// <summary>Lista únicamente perfiles de personal con cuenta activa.</summary>
    public Task<IReadOnlyList<Empleado>> ListarAsync(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        lock (_candado) return Task.FromResult<IReadOnlyList<Empleado>>(_personal.Values
            .Where(x => _cuentas.TryGetValue(x.UsuarioId, out var cuenta) && cuenta.Activo)
            .OrderBy(x => x.Apellidos).ToArray());
    }

    /// <summary>Localiza al empleado, mecánico o gerente asociado a la sesión.</summary>
    Task<Empleado?> IRepositorioPersonal.ObtenerPorUsuarioAsync(ulong usuarioId, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        lock (_candado) return Task.FromResult(_cuentas.TryGetValue(usuarioId, out var cuenta)
            && cuenta.Activo ? _personal.Values.FirstOrDefault(x => x.UsuarioId == usuarioId) : null);
    }

    /// <summary>Comprueba el VIN y el propietario activo antes del alta de una motocicleta.</summary>
    public Task<ulong> RegistrarAsync(Motocicleta motocicleta, ulong usuarioActorId,
        CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        lock (_candado)
        {
            if (!_clientes.TryGetValue(motocicleta.ClienteId, out var cliente)
                || !_cuentas.TryGetValue(cliente.UsuarioId, out var cuenta) || !cuenta.Activo)
                throw new KeyNotFoundException("El propietario no existe o está inactivo.");
            if (_motocicletas.Values.Any(x => string.Equals(x.NumeroSerie,
                motocicleta.NumeroSerie, StringComparison.OrdinalIgnoreCase)))
                throw new InvalidOperationException("El VIN/chasis ya está registrado.");
            ulong id = _motocicletaSiguiente++;
            _motocicletas[id] = motocicleta with { Id = id, RegistradaEn = DateTime.UtcNow };
            RegistrarCambio(usuarioActorId, "motocicletas", id, "ALTA_MOTOCICLETA", null,
                new { id, motocicleta.ClienteId, motocicleta.NumeroSerie });
            return Task.FromResult(id);
        }
    }

    /// <summary>Obtiene una motocicleta para verificar su pertenencia a un cliente.</summary>
    Task<Motocicleta?> IRepositorioMotocicletas.ObtenerPorIdAsync(ulong id, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        lock (_candado) return Task.FromResult(_motocicletas.GetValueOrDefault(id));
    }

    /// <summary>Filtra exclusivamente las motocicletas del propietario autenticado.</summary>
    public Task<IReadOnlyList<Motocicleta>> ListarPorClienteAsync(ulong clienteId,
        CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        lock (_candado) return Task.FromResult<IReadOnlyList<Motocicleta>>(_motocicletas.Values
            .Where(x => x.ClienteId == clienteId).OrderByDescending(x => x.RegistradaEn).ToArray());
    }

    /// <summary>Simula una transacción que exige moto propia y mecánico disponible.</summary>
    public Task<ulong> CrearAsync(AtencionTaller atencion, ulong usuarioActorId, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        lock (_candado)
        {
            if (!_motocicletas.TryGetValue(atencion.MotocicletaId, out var moto)
                || moto.ClienteId != atencion.ClienteId
                || !_clientes.TryGetValue(atencion.ClienteId, out var cliente)
                || !_cuentas.TryGetValue(cliente.UsuarioId, out var cuenta) || !cuenta.Activo
                || !_personal.TryGetValue(atencion.MecanicoId, out var mecanico)
                || !mecanico.EsMecanico || !mecanico.Disponible)
                throw new InvalidOperationException("El cliente, moto o mecánico no es válido.");
            if (atencion.CitaId.HasValue)
                throw new InvalidOperationException("Las citas no están implementadas en esta demostración parcial.");
            ulong id = _atencionSiguiente++;
            _atenciones[id] = atencion with { Id = id, Estado = "RECIBIDA",
                IngresadaEn = DateTime.UtcNow };
            RegistrarCambio(usuarioActorId, "atenciones", id, "RECEPCION", null,
                new { id, atencion.ClienteId, atencion.MotocicletaId, atencion.MecanicoId });
            return Task.FromResult(id);
        }
    }

    /// <summary>Obtiene la atención por su identificador para verificar permisos y estados.</summary>
    public Task<AtencionTaller?> ObtenerAsync(ulong atencionId, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        lock (_candado) return Task.FromResult(_atenciones.GetValueOrDefault(atencionId));
    }

    /// <summary>Guarda los hallazgos y cambia el estado de manera atómica.</summary>
    public Task<bool> RegistrarDiagnosticoAsync(ulong atencionId, string hallazgos,
        ulong usuarioActorId, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        lock (_candado)
        {
            if (!_atenciones.TryGetValue(atencionId, out var atencion) || atencion.Estado != "RECIBIDA")
                return Task.FromResult(false);
            _atenciones[atencionId] = atencion with { TextoDiagnostico = hallazgos,
                Estado = "DIAGNOSTICADA", DiagnosticadaEn = DateTime.UtcNow };
            RegistrarCambio(usuarioActorId, "atenciones", atencionId, "DIAGNOSTICO",
                new { estado = "RECIBIDA" }, new { estado = "DIAGNOSTICADA", diagnostico = hallazgos });
            return Task.FromResult(true);
        }
    }

    /// <summary>Cobra una sola vez con tarifa fija y sin mezclar presupuestos de reparación.</summary>
    public Task<ulong> CobrarRevisionAsync(ulong atencionId, ulong usuarioActorId,
        string? referenciaComprobante, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        lock (_candado)
        {
            if (!_atenciones.TryGetValue(atencionId, out var atencion)
                || atencion.Estado != "DIAGNOSTICADA")
                throw new InvalidOperationException("La atención debe estar diagnosticada para cobrar.");
            if (_cobros.Values.Any(x => x.AtencionId == atencionId))
                throw new InvalidOperationException("La revisión ya tiene un cobro registrado.");
            ulong id = _cobroSiguiente++;
            _cobros[id] = new CobroDemostracion(id, atencionId, TarifaFicticia,
                DateTime.UtcNow, referenciaComprobante);
            RegistrarCambio(usuarioActorId, "cobros_revision", id, "COBRO_REVISION", null,
                new { id, atencionId, importe = TarifaFicticia });
            return Task.FromResult(id);
        }
    }

    /// <summary>Agrega una entrada de auditoría sin modificar las anteriores.</summary>
    public Task RegistrarAsync(RegistroAuditoria registro, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        lock (_candado)
        {
            _auditoria.Add(registro with { Id = _auditoriaSiguiente++, RegistradaEn = DateTime.UtcNow });
            return Task.CompletedTask;
        }
    }

    /// <summary>Consulta los cambios recientes con un máximo establecido por el Handler.</summary>
    public Task<IReadOnlyList<RegistroAuditoria>> ConsultarAsync(int limite, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        lock (_candado) return Task.FromResult<IReadOnlyList<RegistroAuditoria>>(_auditoria
            .OrderByDescending(x => x.Id).Take(limite).ToArray());
    }

    /// <summary>Devuelve cuentas exclusivamente ficticias para alternar los roles en la exposición.</summary>
    public Task<IReadOnlyList<AccesoDemostracion>> ObtenerAccesosAsync(CancellationToken cancelacion)
    {
        cancelacion.ThrowIfCancellationRequested();
        return Task.FromResult<IReadOnlyList<AccesoDemostracion>>(_accesos.ToArray());
    }

    /// <summary>Devuelve exclusivamente datos de apoyo de la demostración, nunca hashes.</summary>
    public Task<ResumenDemostracion> ObtenerResumenAsync(CancellationToken cancelacion)
    {
        cancelacion.ThrowIfCancellationRequested();
        lock (_candado)
        {
            var clientes = _clientes.Values.Select(x => new ClienteDemostracion(x.Id,
                x.UsuarioId, x.Nombres, x.Apellidos, _cuentas[x.UsuarioId].Identificador,
                _cuentas[x.UsuarioId].Activo)).OrderBy(x => x.Apellidos).ToArray();
            return Task.FromResult(new ResumenDemostracion(clientes,
                _motocicletas.Values.OrderBy(x => x.Id).ToArray(),
                _personal.Values.OrderBy(x => x.Id).ToArray(),
                _atenciones.Values.OrderByDescending(x => x.Id).ToArray(),
                _cobros.Values.OrderByDescending(x => x.Id).ToArray(), _auditoria.Count,
                TarifaFicticia));
        }
    }
}

/// <summary>Registra las implementaciones temporales respetando los contratos existentes.</summary>
public static class DependenciasDemostracion
{
    /// <summary>Sustituye los repositorios MySQL sin alterar Commands, Handlers ni Validators.</summary>
    public static IServiceCollection AddDemoInfrastructure(this IServiceCollection servicios)
    {
        servicios.AddSingleton<IProtectorContrasenas, ProtectorContrasenas>();
        servicios.AddSingleton<DatosEnMemoria>();
        servicios.AddSingleton<IRepositorioCuentas>(s => s.GetRequiredService<DatosEnMemoria>());
        servicios.AddSingleton<IRepositorioClientes>(s => s.GetRequiredService<DatosEnMemoria>());
        servicios.AddSingleton<IRepositorioPersonal>(s => s.GetRequiredService<DatosEnMemoria>());
        servicios.AddSingleton<IRepositorioMotocicletas>(s => s.GetRequiredService<DatosEnMemoria>());
        servicios.AddSingleton<IRepositorioAtenciones>(s => s.GetRequiredService<DatosEnMemoria>());
        servicios.AddSingleton<IRepositorioAuditoria>(s => s.GetRequiredService<DatosEnMemoria>());
        servicios.AddSingleton<IConsultaDemostracion>(s => s.GetRequiredService<DatosEnMemoria>());
        servicios.AddScoped<IServicioAutenticacion, ServicioAutenticacion>();
        return servicios;
    }
}
