namespace FollowMotors.Application.Services.Clientes;

using FollowMotors.Application.Abstractions.Autenticacion;
using FollowMotors.Application.Abstractions.Repositories.Clientes;
using FollowMotors.Application.Abstractions.Operaciones;
using FollowMotors.Application.Commands.Clientes;
using FollowMotors.Domain.Entities.Clientes;
using FollowMotors.Domain.Rules.Seguridad;

/// <summary>Coordina el alta presencial y la baja lógica de clientes.</summary>
public sealed class RegistroCliente : IRegistroCliente
{
    private readonly IRepositorioClientes _clientes;
    private readonly IProtectorContrasenas _protector;
    private readonly IUsuarioActual _usuario;
    private readonly IPoliticaPermisos _permisos;

    /// <summary>Recibe únicamente abstracciones mediante inyección de dependencias.</summary>
    public RegistroCliente(IRepositorioClientes clientes, IProtectorContrasenas protector,
        IUsuarioActual usuario, IPoliticaPermisos permisos)
    {
        _clientes = clientes;
        _protector = protector;
        _usuario = usuario;
        _permisos = permisos;
    }

    /// <summary>Crea cuenta y expediente en una transacción administrada por el repositorio.</summary>
    public Task<ulong> CrearAsync(CrearCuentaClienteCommand solicitud, CancellationToken ct)
    {
        if (!_permisos.PuedeRegistrarClientes(_usuario.Rol) || _usuario.UsuarioId is null)
            throw new UnauthorizedAccessException("Solo el personal autorizado registra clientes.");
        ulong actorId = _usuario.UsuarioId.Value;
        var cuenta = new CuentaCliente(0, solicitud.Identificador.Trim().ToLowerInvariant(),
            _protector.GenerarHash(solicitud.Contrasena), "CLIENTE", true, DateTime.UtcNow);
        var cliente = new Cliente(0, 0, solicitud.Nombres.Trim(), solicitud.Apellidos.Trim(),
            solicitud.Telefono?.Trim(), actorId, DateTime.UtcNow);
        return _clientes.CrearAsync(cuenta, cliente, actorId, ct);
    }

    /// <summary>Desactiva una cuenta solo con el rol GERENTE; conserva su historial.</summary>
    public Task<bool> DesactivarAsync(ulong clienteId, CancellationToken ct)
    {
        if (!_permisos.PuedeDesactivarCliente(_usuario.Rol) || _usuario.UsuarioId is null)
            throw new UnauthorizedAccessException("Solo el gerente puede desactivar clientes.");
        return _clientes.DesactivarAsync(clienteId, _usuario.UsuarioId.Value, ct);
    }
}
