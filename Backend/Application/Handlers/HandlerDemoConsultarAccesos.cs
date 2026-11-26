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
