namespace FollowMotors.Application.Handlers.Motocicletas;

using FollowMotors.Application.Abstractions.Autenticacion;
using FollowMotors.Application.Abstractions.Repositories.Clientes;
using FollowMotors.Application.Abstractions.Repositories.Motocicletas;
using FollowMotors.Application.Commands.Motocicletas;
using FollowMotors.Application.Queries.Motocicletas;
using FollowMotors.Domain.Entities.Motocicletas;
using FollowMotors.Domain.Rules.Seguridad;
using MediatR;

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

/// <summary>Asocia motocicletas multimarca solo por personal autorizado.</summary>
public sealed class RegistrarMotocicletaHandler(IRepositorioMotocicletas repositorio,
    IUsuarioActual usuario, IPoliticaPermisos permisos)
    : IRequestHandler<RegistrarMotocicletaCommand, ulong>
{
    /// <summary>Registra la motocicleta por contrato y audita el alta mediante persistencia.</summary>
    public Task<ulong> Handle(RegistrarMotocicletaCommand request, CancellationToken cancellationToken)
    {
        if (!permisos.PuedeRegistrarClientes(usuario.Rol) || usuario.UsuarioId is null)
            throw new UnauthorizedAccessException("Solo el personal puede registrar motocicletas.");
        var moto = new Motocicleta(0, request.ClienteId, request.NumeroSerie.Trim().ToUpperInvariant(),
            request.Placa?.Trim(), request.Marca.Trim(), request.Modelo.Trim(), request.Anio,
            request.Color?.Trim(), request.KilometrajeActual, DateTime.UtcNow);
        return repositorio.RegistrarAsync(moto, usuario.UsuarioId.Value, cancellationToken);
    }
}
