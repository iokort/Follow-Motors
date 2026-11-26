namespace FollowMotors.Application.Handlers.Motocicletas;

using MediatR;
using FollowMotors.Application.Abstractions.Autenticacion;
using FollowMotors.Application.Abstractions.Repositories.Motocicletas;
using FollowMotors.Application.Commands.Motocicletas;
using FollowMotors.Domain.Entities.Motocicletas;
using FollowMotors.Domain.Rules.Seguridad;

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
