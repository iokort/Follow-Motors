namespace FollowMotors.Application.Abstractions.Repositories.Motocicletas;

using FollowMotors.Domain.Entities.Motocicletas;

/// <summary>Contrato para persistir motocicletas y consultar solo unidades propias.</summary>
public interface IRepositorioMotocicletas
{
    /// <summary>Vincula una motocicleta al propietario y registra la operación en auditoría.</summary>
    Task<ulong> RegistrarAsync(Motocicleta motocicleta, ulong usuarioActorId, CancellationToken ct);

    /// <summary>Busca una motocicleta por identificador.</summary>
    Task<Motocicleta?> ObtenerPorIdAsync(ulong id, CancellationToken ct);

    /// <summary>Lista las unidades que pertenecen al propietario indicado.</summary>
    Task<IReadOnlyList<Motocicleta>> ListarPorClienteAsync(ulong clienteId, CancellationToken ct);

    /// <summary>Busca unidades por VIN, marca o modelo, con paginación.</summary>
    Task<IReadOnlyList<Motocicleta>> BuscarAsync(string? texto, int pagina, CancellationToken ct);

    /// <summary>Actualiza la ficha sin reasignar el historial de un propietario a otro.</summary>
    Task ActualizarAsync(Motocicleta motocicleta, ulong actorId, CancellationToken ct);
}
