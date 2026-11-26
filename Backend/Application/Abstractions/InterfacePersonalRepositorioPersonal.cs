namespace FollowMotors.Application.Abstractions.Repositories.Personal;

using FollowMotors.Domain.Entities.Personal;

/// <summary>Contrato de administración y consulta del personal operativo.</summary>
public interface IRepositorioPersonal
{
    /// <summary>Registra a un empleado o mecánico y audita el alta.</summary>
    Task<ulong> RegistrarAsync(string identificador, string hashContrasena,
        string nombres, string apellidos, bool esMecanico, ulong usuarioActorId, CancellationToken ct);

    /// <summary>Consulta los perfiles del personal sin incluir credenciales.</summary>
    Task<IReadOnlyList<Empleado>> ListarAsync(CancellationToken ct);

    /// <summary>Busca al empleado asociado a una cuenta autenticada.</summary>
    Task<Empleado?> ObtenerPorUsuarioAsync(ulong usuarioId, CancellationToken ct);
}
