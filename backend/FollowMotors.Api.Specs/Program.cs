using FollowMotors.Api.Application.Agenda;
using FollowMotors.Api.Domain.Agenda;

// Pruebas de casos de uso sin servicios externos: FakeAgendaRepository sustituye
// MySQL gracias a la inversión de dependencias (SOLID D). Si estas pruebas fallan,
// el proceso sale con código distinto de cero y GitHub Actions rechaza el PR.
var test = new AgendaSpecs();
await test.RunAsync();
Console.WriteLine("US06/US15: pruebas funcionales de negocio correctas.");

internal sealed class AgendaSpecs
{
    private readonly Guid _actor = Guid.NewGuid();
    private readonly DateTime _tomorrow;
    private readonly InMemoryAgendaRepository _repository = new();
    private readonly AgendaService _service;

    public AgendaSpecs()
    {
        var now = new DateTimeOffset(2026, 11, 5, 12, 0, 0, TimeSpan.Zero);
        var clock = new FixedTimeProvider(now);
        _tomorrow = new DateTime(2026, 11, 6, 11, 0, 0, DateTimeKind.Unspecified);
        _service = new AgendaService(_repository, new AgendaPolicy(clock));
    }

    public async Task RunAsync()
    {
        await CrearMotoExternaAsync();
        await CrearMotoCfMotoAsync();
        await DatosInvalidosAsync();
        await ConsultarDiaAsync();
        await ReprogramarAsync();
        await CancelarAsync();
        await ControlarConcurrenciaAsync();
        await EvitarCambiosTerminalesAsync();
    }

    private async Task CrearMotoExternaAsync()
    {
        var cita = await _service.CrearAsync(Externa(), _actor, default);
        Check(cita.Estado == Cita.Programada, "La cita nueva debe ser PROGRAMADA.");
        Check(cita.MotocicletaId is null, "Una moto externa no se registra en el portal.");
        Check(cita.TipoMotocicleta == "EXTERNA", "Se conserva la marca externa.");
        Check(cita.RegistradaPor == _actor, "El actor no procede del request.");
        Check(_repository.Registros == 1, "Se persistió una cita.");
    }

    private async Task CrearMotoCfMotoAsync()
    {
        var id = Guid.NewGuid();
        var cita = await _service.CrearAsync(
            new CrearCitaRequest("CFMOTO", id, null, null, null, null,
                _tomorrow, 60, "Primer mantenimiento"), _actor, default);
        Check(cita.MotocicletaId == id, "La cita CFMOTO usa una moto registrada.");
        Check(cita.MarcaExterna is null, "No se mezclan datos de marcas externas.");
    }

    private async Task DatosInvalidosAsync()
    {
        await ShouldFailAsync(400, () => _service.CrearAsync(
            Externa() with { MotocicletaId = Guid.NewGuid() }, _actor, default));
        await ShouldFailAsync(400, () => _service.CrearAsync(
            Externa() with { Motivo = " " }, _actor, default));
        await ShouldFailAsync(400, () => _service.CrearAsync(
            Externa() with { DuracionMinutos = 10 }, _actor, default));
        await ShouldFailAsync(400, () => _service.CrearAsync(
            Externa() with { InicioLocal = new DateTime(2026, 11, 3, 11, 0, 0) }, _actor, default));
        await ShouldFailAsync(400, () => _service.CrearAsync(
            Externa() with { TipoMotocicleta = "OTRA" }, _actor, default));
        await ShouldFailAsync(401, () => _service.CrearAsync(Externa(), Guid.Empty, default));
    }

    private async Task ConsultarDiaAsync()
    {
        var citas = await _service.ListarDiaAsync(DateOnly.FromDateTime(_tomorrow), default);
        Check(citas.Count >= 2, "US15 permite listar citas de un día.");
        var detalle = await _service.BuscarAsync(citas[0].Id, default);
        Check(detalle is not null, "US15 permite consultar detalle.");
        Check(await _service.BuscarAsync(Guid.NewGuid(), default) is null, "Una cita inexistente devuelve 404 en controlador.");
    }

    private async Task ReprogramarAsync()
    {
        var cita = await _service.CrearAsync(Externa(), _actor, default);
        var nuevoInicio = _tomorrow.AddHours(3);
        var res = await _service.ReprogramarAsync(cita.Id,
            new ReprogramarCitaRequest(nuevoInicio, 90), _actor, default);
        Check(res?.InicioLocal == nuevoInicio && res.DuracionMinutos == 90,
            "US15 reprograma sin borrar la cita.");
        Check(_repository.Changes > 0, "La modificación llega al repositorio.");
    }

    private async Task CancelarAsync()
    {
        var cita = await _service.CrearAsync(Externa(), _actor, default);
        var res = await _service.CancelarAsync(
            cita.Id, new CancelarCitaRequest("El cliente avisó por WhatsApp."), _actor, default);
        Check(res?.Estado == Cita.Cancelada, "US15 conserva cancelación lógica.");
        Check(_repository.LastMotivo == "El cliente avisó por WhatsApp.",
            "US15 registra motivo para auditoría.");
    }

    private async Task ControlarConcurrenciaAsync()
    {
        var cita = await _service.CrearAsync(Externa(), _actor, default);
        _repository.NextWriteFails = true;
        await ShouldFailAsync(409, () => _service.ReprogramarAsync(
            cita.Id, new ReprogramarCitaRequest(_tomorrow.AddHours(6), 60), _actor, default));
    }

    private async Task EvitarCambiosTerminalesAsync()
    {
        var cita = await _service.CrearAsync(Externa(), _actor, default);
        await _service.CancelarAsync(cita.Id, new CancelarCitaRequest("Cita duplicada"), _actor, default);
        await ShouldFailAsync(409, () => _service.ReprogramarAsync(
            cita.Id, new ReprogramarCitaRequest(_tomorrow.AddHours(1), 60), _actor, default));
        await ShouldFailAsync(409, () => _service.CancelarAsync(
            cita.Id, new CancelarCitaRequest("Otra vez"), _actor, default));
    }

    private CrearCitaRequest Externa() =>
        new("EXTERNA", null, "Honda", "CB125", "Cliente de prueba", "0000000000", _tomorrow, 60, "Diagnóstico");

    private static async Task ShouldFailAsync(int expected, Func<Task> action)
    {
        try { await action(); }
        catch (AgendaException ex) when (ex.StatusCode == expected) { return; }
        throw new Exception($"Se esperaba error funcional HTTP {expected}.");
    }

    private static void Check(bool condition, string description)
    {
        if (!condition) throw new Exception($"Prueba fallida: {description}");
        Console.WriteLine($"OK: {description}");
    }
}

/// <summary>Reloj estable para que los tests no dependan del día en que se ejecutan.</summary>
internal sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
{
    public override DateTimeOffset GetUtcNow() => now;
}

/// <summary>
/// Fake que reemplaza al repositorio MySQL sin cambiar AgendaService.
/// Solo sirve para probar reglas; los SQL reales se comprueban por separado.
/// </summary>
internal sealed class InMemoryAgendaRepository : IAgendaRepository
{
    private readonly Dictionary<Guid, Cita> _citas = new();
    public int Registros => _citas.Count;
    public int Changes { get; private set; }
    public string? LastMotivo { get; private set; }
    public bool NextWriteFails { get; set; }

    public Task<Cita?> BuscarAsync(Guid id, CancellationToken ct) =>
        Task.FromResult(_citas.TryGetValue(id, out var cita) ? cita : null);

    public Task<IReadOnlyList<Cita>> ListarDiaAsync(DateOnly dia, CancellationToken ct) =>
        Task.FromResult<IReadOnlyList<Cita>>(
            _citas.Values.Where(x => DateOnly.FromDateTime(x.InicioLocal) == dia).ToList());

    public Task RegistrarAsync(Cita cita, CancellationToken ct)
    {
        _citas.Add(cita.Id, cita);
        return Task.CompletedTask;
    }

    public Task<bool> ReprogramarAsync(Cita anterior, DateTime inicio, int duracion, Guid actor, CancellationToken ct)
    {
        if (NextWriteFails)
        {
            NextWriteFails = false;
            return Task.FromResult(false);
        }
        _citas[anterior.Id] = anterior with { InicioLocal = inicio, DuracionMinutos = duracion };
        Changes++;
        return Task.FromResult(true);
    }

    public Task<bool> CancelarAsync(Cita anterior, string motivo, Guid actor, CancellationToken ct)
    {
        _citas[anterior.Id] = anterior with { Estado = Cita.Cancelada };
        LastMotivo = motivo;
        Changes++;
        return Task.FromResult(true);
    }
}
