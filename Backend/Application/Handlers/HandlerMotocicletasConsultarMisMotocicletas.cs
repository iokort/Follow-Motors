namespace FollowMotors.Application.Handlers.Motocicletas;

using MediatR;
using FollowMotors.Application.Abstractions.Autenticacion;
using FollowMotors.Application.Abstractions.Repositories.Clientes;
using FollowMotors.Application.Abstractions.Repositories.Motocicletas;
using FollowMotors.Application.Queries.Motocicletas;
using FollowMotors.Domain.Entities.Motocicletas;

/// <summary>Obtiene las motocicletas del propietario autenticado sin aceptar IDs arbitrarios.</summary>
public sealed class ConsultarMisMotocicletasHandler(IUsuarioActual usuario,
    IRepositorioClientes clientes, IRepositorioMotocicletas motocicletas)
    : IRequestHandler<ConsultarMisMotocicletasQuery, IReadOnlyList<Motocicleta>>
{
    /// <summary>Resuelve el cliente desde la identidad real y filtra por propietario.</summary>
    public async Task<IReadOnlyList<Motocicleta>> Handle(ConsultarMisMotocicletasQuery request,
        CancellationToken cancellationToken)
    {
        if (usuario.Rol != "CLIENTE" || usuario.UsuarioId is null)
            throw new UnauthorizedAccessException("Se requiere una sesión de cliente.");
        var cliente = await clientes.ObtenerPorUsuarioAsync(usuario.UsuarioId.Value, cancellationToken)
            ?? throw new KeyNotFoundException("No existe el expediente del cliente.");
        return await motocicletas.ListarPorClienteAsync(cliente.Id, cancellationToken);
    }
}
