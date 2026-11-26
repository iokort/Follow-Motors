namespace FollowMotors.Domain.Rules.Cancelaciones;

/// <summary>Calcula el reembolso de anticipo sin incluir el cobro del diagnóstico.</summary>
public sealed class CalculadoraReembolso
{
    /// <summary>Devuelve el reembolso disponible descontando trabajo y piezas usados.</summary>
    public decimal Calcular(decimal anticipoReparacion, decimal manoObraAcordada,
        decimal fraccionTrabajoRealizada, decimal piezasUsadas)
    {
        if (anticipoReparacion < 0 || manoObraAcordada < 0 || piezasUsadas < 0 ||
            fraccionTrabajoRealizada is < 0m or > 1m)
            throw new ArgumentOutOfRangeException(nameof(fraccionTrabajoRealizada), "Los importes y el avance deben ser válidos.");
        decimal retencion = manoObraAcordada * fraccionTrabajoRealizada + piezasUsadas;
        return Math.Clamp(anticipoReparacion - retencion, 0m, anticipoReparacion);
    }
}

/// <summary>Establece el límite de cancelación de una reparación contratada.</summary>
public sealed class PoliticaCancelacion
{
    /// <summary>Permite cancelar un trabajo únicamente con avance verificable de hasta 60 %.</summary>
    public bool SePuedeCancelar(decimal avance, bool yaTerminada) =>
        !yaTerminada && avance is >= 0m and <= 60m;
}
