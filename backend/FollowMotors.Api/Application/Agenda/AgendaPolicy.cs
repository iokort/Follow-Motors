namespace FollowMotors.Api.Application.Agenda;

/// <summary>
/// Reglas puras de agenda, independientes de HTTP y SQL (S y O de SOLID).
/// Estas comprobaciones complementan los CHECK/ENUM de MySQL V3 de Gael;
/// no cambian sus tablas ni inventan una duración fija por tipo de servicio.
/// </summary>
public sealed class AgendaPolicy(TimeProvider clock)
{
    private static readonly TimeZoneInfo Sucursal =
        TimeZoneInfo.FindSystemTimeZoneById("America/Mexico_City");

    /// <summary>Hora local real de la sucursal, obtenida del reloj inyectado.</summary>
    public DateTime AhoraLocal => TimeZoneInfo.ConvertTime(clock.GetUtcNow(), Sucursal).DateTime;

    /// <summary>
    /// Un DateTime de ASP.NET Core puede venir con zona UTC o local; no aceptamos
    /// conversiones silenciosas porque el contrato espera hora local sin offset.
    /// </summary>
    public void ValidarHorario(DateTime inicio, int minutos)
    {
        if (inicio.Kind != DateTimeKind.Unspecified)
            throw new AgendaException("Envía inicioLocal como hora local sin Z ni offset.");

        if (minutos is < 15 or > 480)
            throw new AgendaException("La duración debe estar entre 15 y 480 minutos.");

        if (inicio < AhoraLocal)
            throw new AgendaException("No se puede programar una cita en el pasado.");
    }

    /// <summary>Campos de US06 y restricciones ck_citas_tipo / ck_citas_motivo.</summary>
    public void ValidarNueva(CrearCitaRequest solicitud)
    {
        ValidarHorario(solicitud.InicioLocal, solicitud.DuracionMinutos);

        if (string.IsNullOrWhiteSpace(solicitud.Motivo) || solicitud.Motivo.Trim().Length > 400)
            throw new AgendaException("El motivo es obligatorio (máximo 400 caracteres).");

        if (solicitud.TipoMotocicleta == "CFMOTO")
        {
            if (solicitud.MotocicletaId is null || solicitud.MotocicletaId == Guid.Empty)
                throw new AgendaException("Una cita CFMOTO requiere motocicletaId.");

            if (!string.IsNullOrWhiteSpace(solicitud.MarcaExterna) ||
                !string.IsNullOrWhiteSpace(solicitud.ModeloExterno) ||
                !string.IsNullOrWhiteSpace(solicitud.ContactoExterno))
                throw new AgendaException("Una cita CFMOTO no recibe datos de moto externa.");

            return;
        }

        if (solicitud.TipoMotocicleta != "EXTERNA")
            throw new AgendaException("tipoMotocicleta debe ser CFMOTO o EXTERNA.");

        if (solicitud.MotocicletaId is not null)
            throw new AgendaException("Una cita EXTERNA no vincula una motocicleta del portal.");

        RevisarTexto(solicitud.MarcaExterna, 100, "marcaExterna");
        RevisarTexto(solicitud.ModeloExterno, 150, "modeloExterno");
        RevisarTexto(solicitud.ContactoExterno, 150, "contactoExterno");

        if (solicitud.TelefonoExterno?.Length > 25)
            throw new AgendaException("telefonoExterno supera 25 caracteres.");
    }

    private static void RevisarTexto(string? texto, int max, string campo)
    {
        if (string.IsNullOrWhiteSpace(texto) || texto.Trim().Length > max)
            throw new AgendaException($"{campo} es obligatorio (máximo {max} caracteres).");
    }

    /// <summary>No inventar estados: respetar ENUM de MySQL V3.</summary>
    public static void ValidarModificable(string estado)
    {
        if (estado is not ("PROGRAMADA" or "CONFIRMADA"))
            throw new AgendaException("La cita ya está realizada o cancelada.", 409);
    }

    /// <summary>Las cancelaciones conservan el motivo en auditoría.</summary>
    public static string MotivoCancelacion(string? motivo)
    {
        if (string.IsNullOrWhiteSpace(motivo) || motivo.Trim().Length > 500)
            throw new AgendaException("Indica motivo de cancelación (máximo 500 caracteres).");
        return motivo.Trim();
    }
}
