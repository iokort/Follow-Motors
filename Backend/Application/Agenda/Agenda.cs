namespace FollowMotors.Application.Agenda;

using FollowMotors.Application.Operaciones;
using MediatR;

/// <summary>Franja de recepción expresada en UTC, con sus cupos y estado actuales.</summary>
public sealed record FranjaAgenda(ulong Id, DateTime Inicio, DateTime Fin, ushort Capacidad,
    bool Habilitada, long ReservasConfirmadas);

/// <summary>Cita del expediente, con horario, origen y estado de atención.</summary>
public sealed record CitaAgenda(ulong Id, ulong FranjaId, ulong ClienteId, ulong? MotocicletaId,
    string Motivo, string Estado, string Origen, DateTime Inicio, DateTime Fin);

/// <summary>Horario y número de revisiones que puede atender el taller en una franja.</summary>
public sealed record DatosFranja(DateTimeOffset Inicio, DateTimeOffset Fin, ushort Capacidad, bool Habilitada = true);

/// <summary>Guarda una franja nueva o modifica la franja indicada.</summary>
public sealed record GuardarFranjaCommand(ulong? Id, DatosFranja Datos) : IRequest<ulong>;

/// <summary>Consulta franjas del rango, opcionalmente limitadas a las que tienen capacidad libre.</summary>
public sealed record FranjasQuery(DateTimeOffset Desde, DateTimeOffset Hasta, bool Disponibles)
    : IRequest<IReadOnlyList<FranjaAgenda>>;

/// <summary>Registra una cita gestionada por el personal para una motocicleta del expediente.</summary>
public sealed record RegistrarCitaCommand(ulong FranjaId, ulong ClienteId, ulong MotocicletaId, string Motivo)
    : IRequest<ulong>;

/// <summary>Traslada una cita confirmada a una franja con cupo.</summary>
public sealed record ReprogramarCitaCommand(ulong Id, ulong FranjaId) : IRequest;

/// <summary>Registra cancelación o inasistencia sin eliminar el historial.</summary>
public sealed record CerrarCitaCommand(ulong Id, bool NoAsistio = false) : IRequest;

/// <summary>Lista citas, limitadas al titular cuando la sesión es de cliente.</summary>
public sealed record CitasQuery(DateTimeOffset? Desde, DateTimeOffset? Hasta, int Pagina = 1)
    : IRequest<IReadOnlyList<CitaAgenda>>;

/// <summary>Consulta la cita indicada si está autorizada para la sesión.</summary>
public sealed record CitaQuery(ulong Id) : IRequest<CitaAgenda>;

/// <summary>Abstrae la lectura y los cambios transaccionales de la agenda.</summary>
public interface IRepositorioAgenda
{
    /// <summary>Persiste el horario sin solapar franjas ni invalidar reservas existentes.</summary>
    Task<ulong> GuardarFranjaAsync(GuardarFranjaCommand solicitud, ulong actorId, CancellationToken ct);

    /// <summary>Consulta franjas y su ocupación real.</summary>
    Task<IReadOnlyList<FranjaAgenda>> FranjasAsync(FranjasQuery consulta, DateTime ahora, CancellationToken ct);

    /// <summary>Reserva el cupo de recepción en una operación serializada por franja.</summary>
    Task<ulong> RegistrarAsync(RegistrarCitaCommand solicitud, ulong actorId, DateTime ahora, CancellationToken ct);

    /// <summary>Traslada la cita y actualiza su confirmación en la misma transacción.</summary>
    Task ReprogramarAsync(ReprogramarCitaCommand solicitud, ulong actorId, DateTime ahora, CancellationToken ct);

    /// <summary>Libera una reserva sin borrar su rastro.</summary>
    Task CerrarAsync(CerrarCitaCommand solicitud, ulong actorId, DateTime ahora, CancellationToken ct);

    /// <summary>Filtra las citas visibles por fecha, identidad y página.</summary>
    Task<IReadOnlyList<CitaAgenda>> CitasAsync(CitasQuery consulta, ActorTaller actor, CancellationToken ct);

    /// <summary>Consulta la cita visible para la sesión o devuelve ausencia.</summary>
    Task<CitaAgenda?> CitaAsync(ulong id, ActorTaller actor, CancellationToken ct);
}
