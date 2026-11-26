namespace FollowMotors.Application.Abstractions.Repositories.Clientes;

using FollowMotors.Domain.Entities.Clientes;

/// <summary>Permite consultar credenciales sin exponer dependencias SQL a Application.</summary>
public interface IRepositorioCuentas
{
    /// <summary>Busca una cuenta para autenticar al usuario.</summary>
    Task<CuentaCliente?> BuscarPorIdentificadorAsync(string identificador, CancellationToken ct);

    /// <summary>Obtiene una cuenta por su ID para verificar si sigue activa.</summary>
    Task<CuentaCliente?> ObtenerPorIdAsync(ulong usuarioId, CancellationToken ct);

    /// <summary>Indica si existe un gerente activo para el arranque inicial seguro.</summary>
    Task<bool> ExisteGerenteAsync(CancellationToken ct);

    /// <summary>Crea el primer gerente únicamente cuando todavía no exista ninguno.</summary>
    Task<bool> CrearGerenteInicialAsync(string identificador, string hashContrasena,
        string nombres, string apellidos, CancellationToken ct);
}
