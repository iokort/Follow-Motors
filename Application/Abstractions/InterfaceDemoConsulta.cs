namespace FollowMotors.Application.Abstractions.Demostracion;

using FollowMotors.Application.Models.Demostracion;

/// <summary>Contrato de lectura de datos ficticios exclusivamente para una demostración local.</summary>
public interface IConsultaDemostracion
{
    /// <summary>Obtiene catálogos de apoyo para los formularios de la interfaz administrativa.</summary>
    Task<ResumenDemostracion> ObtenerResumenAsync(CancellationToken cancelacion);

    /// <summary>Expone credenciales ficticias solo en modo local; nunca existe en producción.</summary>
    Task<IReadOnlyList<AccesoDemostracion>> ObtenerAccesosAsync(CancellationToken cancelacion);
}
