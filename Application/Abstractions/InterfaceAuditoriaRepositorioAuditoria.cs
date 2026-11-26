namespace FollowMotors.Application.Abstractions.Repositories.Auditoria;

using FollowMotors.Domain.Entities.Auditoria;

/// <summary>Permite registrar y consultar los cambios sensibles del sistema.</summary>
public interface IRepositorioAuditoria
{
    /// <summary>Agrega una entrada de auditoría sin alterar registros previos.</summary>
    Task RegistrarAsync(RegistroAuditoria registro, CancellationToken ct);

    /// <summary>Consulta los cambios recientes con un límite de resultados.</summary>
    Task<IReadOnlyList<RegistroAuditoria>> ConsultarAsync(int limite, CancellationToken ct);
}
