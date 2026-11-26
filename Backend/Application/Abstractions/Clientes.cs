namespace FollowMotors.Application.Abstractions.Repositories.Clientes;

using FollowMotors.Domain.Entities.Clientes;
using FollowMotors.Application.Commands.Clientes;
using FollowMotors.Application.Models.Clientes;

/// <summary>Define las operaciones de almacenamiento de propietarios y cuentas vinculadas.</summary>
public interface IRepositorioClientes
{
    /// <summary>Registra cuenta y propietario en una misma transacción.</summary>
    Task<ulong> CrearAsync(CuentaCliente cuenta, Cliente cliente, ulong usuarioActorId, CancellationToken ct);

    /// <summary>Desactiva una cuenta conservando el expediente histórico y auditando el cambio.</summary>
    Task<bool> DesactivarAsync(ulong clienteId, ulong usuarioActorId, CancellationToken ct);

    /// <summary>Obtiene el propietario por el identificador de su cuenta.</summary>
    Task<Cliente?> ObtenerPorUsuarioAsync(ulong usuarioId, CancellationToken ct);

    /// <summary>Obtiene un propietario por su identificador interno.</summary>
    Task<Cliente?> ObtenerPorIdAsync(ulong id, CancellationToken ct);

    /// <summary>Busca expedientes mediante un prefijo parametrizado y una página estable.</summary>
    Task<IReadOnlyList<ClienteFicha>> BuscarAsync(string? texto, int pagina, CancellationToken ct);

    /// <summary>Consulta la ficha sin cargar contraseñas.</summary>
    Task<ClienteFicha?> ObtenerFichaAsync(ulong id, CancellationToken ct);

    /// <summary>Cambia los datos del propietario y registra la acción en la misma transacción.</summary>
    Task ActualizarAsync(ulong id, DatosCliente datos, ulong actorId, CancellationToken ct);
}

/// <summary>Permite consultar credenciales sin exponer dependencias SQL a Application.</summary>
public interface IRepositorioCuentas
{
    /// <summary>Busca una cuenta para autenticar al usuario.</summary>
    Task<CuentaCliente?> BuscarPorIdentificadorAsync(string identificador, CancellationToken ct);

    /// <summary>Obtiene una cuenta por su ID para verificar si sigue activa.</summary>
    Task<CuentaCliente?> ObtenerPorIdAsync(ulong usuarioId, CancellationToken ct);

    /// <summary>Crea el primer gerente únicamente cuando todavía no exista ninguno.</summary>
    Task<bool> CrearGerenteInicialAsync(string identificador, string hashContrasena,
        string nombres, string apellidos, CancellationToken ct);
}
