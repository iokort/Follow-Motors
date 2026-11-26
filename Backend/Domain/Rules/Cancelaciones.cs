namespace FollowMotors.Domain.Rules.Cancelaciones;

using FollowMotors.Domain.Rules.Operaciones;

/// <summary>Desglose de la liquidación del anticipo de reparación.</summary>
public sealed record LiquidacionCancelacion(decimal ManoObraEjecutada, decimal PiezasUsadas, decimal Reembolso);

/// <summary>Calcula el límite de cancelación y el reintegro sin incorporar la revisión fija.</summary>
public static class PoliticaCancelacion
{
    /// <summary>Permite aprobar cancelación hasta 60 % verificado, inclusive.</summary>
    public static bool AvanceCancelable(decimal avance) => avance is >= 0m and <= 60m;

    /// <summary>Descuenta mano de obra verificada y piezas consumidas del monto efectivamente pagado.</summary>
    /// <exception cref="ArgumentOutOfRangeException">Los importes o el avance no representan una liquidación válida.</exception>
    public static LiquidacionCancelacion Liquidar(decimal pagado, decimal manoObra, decimal avance, decimal piezasUsadas)
    {
        if (pagado < 0 || manoObra < 0 || piezasUsadas < 0 || !AvanceCancelable(avance))
            throw new ArgumentOutOfRangeException(nameof(avance), "La cancelación contiene importes o un avance no válidos.");
        decimal trabajo = ImportesReparacion.Redondear(manoObra * avance / 100m);
        return new LiquidacionCancelacion(trabajo, piezasUsadas, Math.Max(0m, pagado - trabajo - piezasUsadas));
    }
}
