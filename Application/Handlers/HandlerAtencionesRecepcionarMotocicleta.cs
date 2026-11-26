namespace FollowMotors.Application.Handlers.Atenciones;

using MediatR;
using FollowMotors.Application.Abstractions.Operaciones;
using FollowMotors.Application.Commands.Atenciones;

/// <summary>Coordina el caso de uso RecepcionarMotocicleta por medio de una interfaz.</summary>
public sealed class RecepcionarMotocicletaHandler(IRecepcionTaller recepcion)
    : IRequestHandler<RecepcionarMotocicletaCommand, ulong>
{
    /// <summary>Ejecuta la operación sin acceder directamente a MySQL ni HTTP.</summary>
    public Task<ulong> Handle(RecepcionarMotocicletaCommand request, CancellationToken cancellationToken) =>
        recepcion.RecibirAsync(request, cancellationToken);
}
