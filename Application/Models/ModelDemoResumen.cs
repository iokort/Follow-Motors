namespace FollowMotors.Application.Models.Demostracion;

using FollowMotors.Domain.Entities.Atenciones;
using FollowMotors.Domain.Entities.Motocicletas;
using FollowMotors.Domain.Entities.Personal;

/// <summary>Datos de apoyo sin contraseñas ni información de autenticación sensible.</summary>
public sealed record ClienteDemostracion(ulong Id, ulong UsuarioId, string Nombres,
    string Apellidos, string Identificador, bool Activo);

/// <summary>Importe de un cobro ficticio efectuado durante la demostración.</summary>
public sealed record CobroDemostracion(ulong Id, ulong AtencionId, decimal Importe,
    DateTime Fecha, string? Referencia);

/// <summary>Cuenta ficticia disponible para el inicio rápido de la demostración local.</summary>
public sealed record AccesoDemostracion(string Identificador, string Contrasena,
    string Rol, string Nombre);

/// <summary>Resumen temporal usado para mostrar las operaciones existentes en la UI.</summary>
public sealed record ResumenDemostracion(
    IReadOnlyList<ClienteDemostracion> Clientes,
    IReadOnlyList<Motocicleta> Motocicletas,
    IReadOnlyList<Empleado> Personal,
    IReadOnlyList<AtencionTaller> Atenciones,
    IReadOnlyList<CobroDemostracion> Cobros,
    int Auditorias,
    decimal TarifaRevision);
