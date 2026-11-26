namespace FollowMotors.Domain.Rules.Operaciones;

/// <summary>Verifica importes monetarios compartidos por el acuerdo y el anticipo.</summary>
public static class ImportesReparacion
{
    /// <summary>Comprueba el total positivo y el límite DECIMAL(12,2) de un presupuesto o pago.</summary>
    public static bool EsTotalValido(decimal manoObra, decimal piezas) =>
        manoObra is >= 0m and <= 9_999_999_999.99m && piezas is >= 0m and <= 9_999_999_999.99m
        && manoObra + piezas is > 0m and <= 9_999_999_999.99m;

    /// <summary>Redondea la retención del trabajo ejecutado a centavos.</summary>
    public static decimal Redondear(decimal importe) => Math.Round(importe, 2, MidpointRounding.AwayFromZero);
}

/// <summary>Calcula el estado de cobertura con las fechas del taller y el cierre del servicio.</summary>
public static class VigenciaGarantia
{
    /// <summary>Devuelve el estado efectivo de la cobertura para el instante consultado.</summary>
    public static string Estado(bool aplica, string estadoOrden, DateTime? inicio, DateTime? vence, DateTime ahora)
    {
        if (!aplica || estadoOrden == "CANCELADA") return "SIN_COBERTURA";
        if (estadoOrden != "ENTREGADA" || !inicio.HasValue || !vence.HasValue || ahora < inicio.Value)
            return "INFORMADA";
        return ahora < vence.Value ? "VIGENTE" : "VENCIDA";
    }
}
