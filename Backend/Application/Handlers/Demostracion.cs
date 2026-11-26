namespace FollowMotors.Application.Handlers.Demostracion;

using FollowMotors.Application.Abstractions.Demostracion;
using FollowMotors.Application.Models.Demostracion;
using FollowMotors.Application.Queries.Demostracion;
using MediatR;

/// <summary>Coordina por interfaz la obtención de credenciales exclusivamente ficticias.</summary>
public sealed class ConsultarAccesosDemostracionHandler(IConsultaDemostracion consulta)
    : IRequestHandler<ConsultarAccesosDemostracionQuery, IReadOnlyList<AccesoDemostracion>>
{
    /// <summary>Devuelve únicamente cuentas de ejemplo, sin acceder a MySQL.</summary>
    public Task<IReadOnlyList<AccesoDemostracion>> Handle(ConsultarAccesosDemostracionQuery solicitud,
        CancellationToken cancelacion) => consulta.ObtenerAccesosAsync(cancelacion);
}

/// <summary>Coordina la lectura del catálogo temporal por medio de una interfaz.</summary>
public sealed class ConsultarCatalogosDemostracionHandler(IConsultaDemostracion consulta)
    : IRequestHandler<ConsultarCatalogosDemostracionQuery, ResumenDemostracion>
{
    /// <summary>Solicita datos ficticios sin incluir lógica en el controlador HTTP.</summary>
    public Task<ResumenDemostracion> Handle(ConsultarCatalogosDemostracionQuery solicitud,
        CancellationToken cancelacion) => consulta.ObtenerResumenAsync(cancelacion);
}
