namespace FollowMotors.Application.Handlers.Auditoria;

using MediatR;
using FollowMotors.Application.Abstractions.Autenticacion;
using FollowMotors.Application.Abstractions.Repositories.Auditoria;
using FollowMotors.Application.Queries.Auditoria;
using FollowMotors.Domain.Entities.Auditoria;
using FollowMotors.Domain.Rules.Seguridad;

/// <summary>Consulta de auditoría exclusiva para gerencia.</summary>
public sealed class ConsultarAuditoriaHandler(IRepositorioAuditoria auditoria,
    IUsuarioActual usuario, IPoliticaPermisos permisos)
    : IRequestHandler<ConsultarAuditoriaQuery, IReadOnlyList<RegistroAuditoria>>
{
    /// <summary>Solicita los últimos registros sin permitir consultas ilimitadas.</summary>
    public Task<IReadOnlyList<RegistroAuditoria>> Handle(ConsultarAuditoriaQuery request,
        CancellationToken cancellationToken)
    {
        if (!permisos.PuedeAdministrarPersonal(usuario.Rol))
            throw new UnauthorizedAccessException("Solo el gerente consulta la auditoría.");
        if (request.Limite < 1 || request.Limite > 200)
            throw new ArgumentOutOfRangeException(nameof(request.Limite), "El límite debe estar entre 1 y 200.");
        return auditoria.ConsultarAsync(request.Limite, cancellationToken);
    }
}
