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
