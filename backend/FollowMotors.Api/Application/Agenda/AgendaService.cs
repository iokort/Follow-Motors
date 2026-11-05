using FollowMotors.Api.Domain.Agenda;

namespace FollowMotors.Api.Application.Agenda;

/// <summary>
/// Implementa dos historias sin depender de ASP.NET ni de MySQL concreto:
/// US06 crea citas; US15 las consulta, reprograma y cancela.
/// SRP: decisiones de negocio aquí; SQL en repositorio; HTTP en Controller.
/// </summary>
public sealed class AgendaService(IAgendaRepository repository, AgendaPolicy policy) : IAgendaService
{
    /// <summary>US06: registrar cita para CFMOTO o una moto externa.</summary>
    public async Task<Cita> CrearAsync(CrearCitaRequest request, Guid actor, CancellationToken ct)
    {
        ValidarActor(actor);
        policy.ValidarNueva(request);

        var nueva = new Cita(
            Guid.NewGuid(),
            request.TipoMotocicleta,
            request.MotocicletaId,
            request.MarcaExterna?.Trim(),
            request.ModeloExterno?.Trim(),
            request.ContactoExterno?.Trim(),
            request.TelefonoExterno?.Trim(),
            request.InicioLocal,
            request.DuracionMinutos,
            request.Motivo.Trim(),
            Cita.Programada,
            actor);

        // La transacción y la auditoría se aplican juntas dentro del repositorio.
        await repository.RegistrarAsync(nueva, ct);
        return nueva;
    }

    public Task<Cita?> BuscarAsync(Guid id, CancellationToken ct) => repository.BuscarAsync(id, ct);
    public Task<IReadOnlyList<Cita>> ListarDiaAsync(DateOnly dia, CancellationToken ct) =>
        repository.ListarDiaAsync(dia, ct);

    /// <summary>US15: no cambia citas terminales, y falla ante actualización concurrente.</summary>
    public async Task<Cita?> ReprogramarAsync(Guid id, ReprogramarCitaRequest request, Guid actor, CancellationToken ct)
    {
        ValidarActor(actor);
        policy.ValidarHorario(request.InicioLocal, request.DuracionMinutos);
        var actual = await repository.BuscarAsync(id, ct);
        if (actual is null) return null;
        AgendaPolicy.ValidarModificable(actual.Estado);

        if (actual.InicioLocal == request.InicioLocal && actual.DuracionMinutos == request.DuracionMinutos)
            return actual;

        if (!await repository.ReprogramarAsync(actual, request.InicioLocal, request.DuracionMinutos, actor, ct))
            throw new AgendaException("La cita cambió mientras la editabas; actualiza la página.", 409);

        return actual with { InicioLocal = request.InicioLocal, DuracionMinutos = request.DuracionMinutos };
    }

    /// <summary>US15: cancelación lógica; jamás elimina la cita del historial.</summary>
    public async Task<Cita?> CancelarAsync(Guid id, CancelarCitaRequest request, Guid actor, CancellationToken ct)
    {
        ValidarActor(actor);
        var motivo = AgendaPolicy.MotivoCancelacion(request.Motivo);
        var actual = await repository.BuscarAsync(id, ct);
        if (actual is null) return null;
        AgendaPolicy.ValidarModificable(actual.Estado);

        if (!await repository.CancelarAsync(actual, motivo, actor, ct))
            throw new AgendaException("La cita cambió mientras la editabas; actualiza la página.", 409);

        return actual with { Estado = Cita.Cancelada };
    }

    private static void ValidarActor(Guid id)
    {
        if (id == Guid.Empty)
            throw new AgendaException("No se identificó un miembro del personal autorizado.", 401);
    }
}
