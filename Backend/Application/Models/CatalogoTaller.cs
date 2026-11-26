namespace FollowMotors.Application.Models.Catalogos;

using FollowMotors.Application.Models.Clientes;
using FollowMotors.Domain.Entities.Atenciones;
using FollowMotors.Domain.Entities.Motocicletas;
using FollowMotors.Domain.Entities.Personal;

/// <summary>Importe y fecha de una revisión efectivamente cobrada.</summary>
public sealed record CobroCatalogo(ulong Id, ulong AtencionId, decimal Importe, DateTime Fecha, string? Referencia);

/// <summary>Datos operativos que usa el panel Angular para sus formularios existentes.</summary>
public sealed record CatalogoTaller(
    IReadOnlyList<ClienteFicha> Clientes,
    IReadOnlyList<Motocicleta> Motocicletas,
    IReadOnlyList<Empleado> Personal,
    IReadOnlyList<AtencionTaller> Atenciones,
    IReadOnlyList<CobroCatalogo> Cobros,
    int Auditorias,
    decimal? TarifaRevision);
