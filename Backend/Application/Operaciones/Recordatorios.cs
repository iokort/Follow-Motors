namespace FollowMotors.Application.Operaciones;

using FollowMotors.Application.Abstractions.Autenticacion;
using MediatR;

/// <summary>Preferencia del titular para recibir recordatorios en la bandeja privada.</summary>
public sealed record PreferenciaRecordatorios(bool Habilitado);

/// <summary>Lee el consentimiento actual del cliente.</summary>
public sealed record PreferenciaRecordatoriosQuery : IRequest<PreferenciaRecordatorios>;

/// <summary>Concede o revoca el consentimiento para recordatorios internos.</summary>
public sealed record CambiarPreferenciaRecordatoriosCommand(bool Habilitado) : IRequest;

/// <summary>Consulta avisos próximos generados para el cliente actual.</summary>
public sealed record MisRecordatoriosQuery : IRequest<IReadOnlyList<RecordatorioVista>>;

/// <summary>Inicia una generación de recordatorios mediante gerencia.</summary>
public sealed record GenerarRecordatoriosCommand : IRequest<int>;

/// <summary>Evento próximo con la información indispensable para presentarlo al titular.</summary>
public sealed record RecordatorioVista(ulong Id, string Tipo, ulong EventoId, ulong MotocicletaId,
    string Motocicleta, string Servicio, DateTime FechaEvento, DateTime GeneradoEn);

/// <summary>Horizontes configurables de anticipación para citas y mantenimientos.</summary>
public sealed record VentanaRecordatorios(int HorasCita, int DiasMantenimiento);

/// <summary>Guarda el consentimiento y publica recordatorios internos de registros vigentes.</summary>
public interface IRepositorioRecordatorios
{
    /// <summary>Consulta el consentimiento actual por cuenta.</summary>
    Task<bool> PreferenciaAsync(ulong usuarioId, CancellationToken ct);

    /// <summary>Aplica el consentimiento y deja evidencia de su actualización.</summary>
    Task CambiarPreferenciaAsync(ulong usuarioId, bool habilitado, CancellationToken ct);

    /// <summary>Devuelve únicamente eventos propios, válidos y futuros.</summary>
    Task<IReadOnlyList<RecordatorioVista>> ListarAsync(ulong usuarioId, DateTime ahora, DateTime hoy, CancellationToken ct);

    /// <summary>Inserta recordatorios únicos para los eventos elegibles del intervalo.</summary>
    Task<int> GenerarAsync(DateTime ahora, DateTime hastaCitas, DateTime hoy, DateTime hastaMantenimientos, CancellationToken ct);
}

/// <summary>Aplica las mismas fechas y reglas tanto en ejecución periódica como manual.</summary>
public sealed class GeneradorRecordatorios(IRepositorioRecordatorios repositorio, TimeProvider reloj,
    TimeZoneInfo zona, VentanaRecordatorios ventana)
{
    /// <summary>Calcula las fechas límite para citas UTC y mantenimientos del calendario del taller.</summary>
    public Task<int> GenerarAsync(CancellationToken ct)
    {
        var ahora = reloj.GetUtcNow().UtcDateTime;
        var hoy = TimeZoneInfo.ConvertTimeFromUtc(ahora, zona).Date;
        return repositorio.GenerarAsync(ahora, ahora.AddHours(ventana.HorasCita), hoy,
            hoy.AddDays(ventana.DiasMantenimiento), ct);
    }
}

/// <summary>Autoriza las preferencias privadas, la bandeja y el disparo manual de recordatorios.</summary>
public sealed class RecordatoriosHandler(IRepositorioRecordatorios repositorio, IUsuarioActual usuario,
    GeneradorRecordatorios generador, TimeProvider reloj, TimeZoneInfo zona) : CasoUsoTaller(usuario),
    IRequestHandler<PreferenciaRecordatoriosQuery, PreferenciaRecordatorios>,
    IRequestHandler<CambiarPreferenciaRecordatoriosCommand>,
    IRequestHandler<MisRecordatoriosQuery, IReadOnlyList<RecordatorioVista>>,
    IRequestHandler<GenerarRecordatoriosCommand, int>
{
    /// <summary>Recupera el consentimiento asociado a la identidad del titular.</summary>
    public async Task<PreferenciaRecordatorios> Handle(PreferenciaRecordatoriosQuery consulta, CancellationToken ct) =>
        new(await repositorio.PreferenciaAsync(Titular(), ct));

    /// <summary>Aplica la elección expresa del cliente autenticado.</summary>
    public Task Handle(CambiarPreferenciaRecordatoriosCommand solicitud, CancellationToken ct) =>
        repositorio.CambiarPreferenciaAsync(Titular(), solicitud.Habilitado, ct);

    /// <summary>Lista los avisos vigentes del cliente con referencia a la cita o mantenimiento.</summary>
    public Task<IReadOnlyList<RecordatorioVista>> Handle(MisRecordatoriosQuery consulta, CancellationToken ct)
    {
        var ahora = reloj.GetUtcNow().UtcDateTime;
        return repositorio.ListarAsync(Titular(), ahora, TimeZoneInfo.ConvertTimeFromUtc(ahora, zona).Date, ct);
    }

    /// <summary>Ejecuta la misma generación que el servicio periódico después de verificar gerencia.</summary>
    public Task<int> Handle(GenerarRecordatoriosCommand solicitud, CancellationToken ct)
    {
        Gerente();
        return generador.GenerarAsync(ct);
    }

    /// <summary>Limita las operaciones de la bandeja y consentimiento a cuentas de cliente.</summary>
    private ulong Titular()
    {
        var actor = Actor();
        if (actor.Rol != "CLIENTE") throw new UnauthorizedAccessException("Se requiere una cuenta de cliente.");
        return actor.UsuarioId;
    }
}
