namespace FollowMotors.Application.Handlers.Atenciones;

using MediatR;
using FollowMotors.Application.Abstractions.Operaciones;
using FollowMotors.Application.Commands.Atenciones;

/// <summary>Coordina el caso de uso RegistrarDiagnostico por medio de una interfaz.</summary>
public sealed class RegistrarDiagnosticoHandler(IRecepcionTaller recepcion)
    : IRequestHandler<RegistrarDiagnosticoCommand, bool>
{
    /// <summary>Ejecuta la operación sin acceder directamente a MySQL ni HTTP.</summary>
    public Task<bool> Handle(RegistrarDiagnosticoCommand request, CancellationToken cancellationToken) =>
        recepcion.DiagnosticarAsync(request, cancellationToken);
}
