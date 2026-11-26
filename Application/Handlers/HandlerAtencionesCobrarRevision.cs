namespace FollowMotors.Application.Handlers.Atenciones;

using MediatR;
using FollowMotors.Application.Abstractions.Operaciones;
using FollowMotors.Application.Commands.Atenciones;

/// <summary>Coordina el caso de uso CobrarRevision por medio de una interfaz.</summary>
public sealed class CobrarRevisionHandler(IRecepcionTaller recepcion)
    : IRequestHandler<CobrarRevisionCommand, ulong>
{
    /// <summary>Ejecuta la operación sin acceder directamente a MySQL ni HTTP.</summary>
    public Task<ulong> Handle(CobrarRevisionCommand request, CancellationToken cancellationToken) =>
        recepcion.CobrarAsync(request, cancellationToken);
}
