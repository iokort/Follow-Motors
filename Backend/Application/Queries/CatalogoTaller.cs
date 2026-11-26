namespace FollowMotors.Application.Queries.Catalogos;

using FollowMotors.Application.Abstractions.Autenticacion;
using FollowMotors.Application.Models.Catalogos;
using FollowMotors.Application.Operaciones;
using MediatR;

/// <summary>Solicita los datos operativos para los selectores y tablas de Angular.</summary>
public sealed record ConsultarCatalogoTallerQuery : IRequest<CatalogoTaller>;

/// <summary>Proyección del catálogo a partir de los datos reales del taller.</summary>
public interface IConsultaCatalogoTaller
{
    /// <summary>Recupera el catálogo autorizado y su tarifa vigente.</summary>
    Task<CatalogoTaller> ConsultarAsync(bool incluirConteoAuditoria, CancellationToken ct);
}

/// <summary>Limita el catálogo a personal autenticado.</summary>
public sealed class CatalogoTallerHandler(IConsultaCatalogoTaller catalogo, IUsuarioActual usuario)
    : CasoUsoTaller(usuario), IRequestHandler<ConsultarCatalogoTallerQuery, CatalogoTaller>
{
    /// <summary>Solicita el conteo de auditoría únicamente para el gerente.</summary>
    public Task<CatalogoTaller> Handle(ConsultarCatalogoTallerQuery solicitud, CancellationToken ct) =>
        catalogo.ConsultarAsync(Empleado().Rol == "GERENTE", ct);
}
