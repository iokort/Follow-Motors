using FollowMotors.Api.Domain.Agenda;

namespace FollowMotors.Api.Application.Agenda;

/// <summary>
/// Contrato HTTP de US06: únicamente el gerente/asesor lo envía.
/// Para CFMOTO se requiere motocicletaId; para EXTERNA, marca, modelo y contacto.
/// No se registra automáticamente una moto externa como propiedad de un cliente.
/// </summary>
public sealed record CrearCitaRequest(
    string TipoMotocicleta,
    Guid? MotocicletaId,
    string? MarcaExterna,
    string? ModeloExterno,
    string? ContactoExterno,
    string? TelefonoExterno,
    DateTime InicioLocal,
    int DuracionMinutos,
    string Motivo);

/// <summary>
/// Contrato de US15: solo horario y duración son editables por esta operación.
/// Los demás campos de la cita permanecen intactos.
/// </summary>
public sealed record ReprogramarCitaRequest(DateTime InicioLocal, int DuracionMinutos);

/// <summary>
/// Contrato de US15: se exige un motivo de cancelación para auditoría.
/// No se borra físicamente la cita.
/// </summary>
public sealed record CancelarCitaRequest(string Motivo);

/// <summary>
/// Error funcional controlado: el Controller lo convierte en HTTP sin incluir SQL
/// ni excepciones internas de la base de datos.
/// </summary>
public sealed class AgendaException(string message, int statusCode = 400) : Exception(message)
{
    public int StatusCode { get; } = statusCode;
}

/// <summary>
/// Abstracción de persistencia (D de SOLID): el caso de uso depende de este
/// contrato, no de MySqlConnection. Es posible sustituirlo por un fake en pruebas.
/// </summary>
public interface IAgendaRepository
{
    Task<Cita?> BuscarAsync(Guid id, CancellationToken ct);
    Task<IReadOnlyList<Cita>> ListarDiaAsync(DateOnly dia, CancellationToken ct);
    Task RegistrarAsync(Cita cita, CancellationToken ct);
    Task<bool> ReprogramarAsync(Cita anterior, DateTime inicio, int duracion, Guid actor, CancellationToken ct);
    Task<bool> CancelarAsync(Cita anterior, string motivo, Guid actor, CancellationToken ct);
}

/// <summary>
/// Operaciones del caso de uso; Controller no necesita conocer implementación,
/// MySQL ni políticas de transición (I de SOLID).
/// </summary>
public interface IAgendaService
{
    Task<Cita> CrearAsync(CrearCitaRequest request, Guid actor, CancellationToken ct);
    Task<Cita?> BuscarAsync(Guid id, CancellationToken ct);
    Task<IReadOnlyList<Cita>> ListarDiaAsync(DateOnly dia, CancellationToken ct);
    Task<Cita?> ReprogramarAsync(Guid id, ReprogramarCitaRequest request, Guid actor, CancellationToken ct);
    Task<Cita?> CancelarAsync(Guid id, CancelarCitaRequest request, Guid actor, CancellationToken ct);
}
