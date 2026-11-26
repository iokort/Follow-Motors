namespace FollowMotors.Domain.ValueObjects;

/// <summary>Define la tarifa fija de revisión y su periodo de vigencia.</summary>
public sealed record TarifaRevision(ulong Id, decimal Importe, DateTime VigenteDesde, DateTime? VigenteHasta)
{
    /// <summary>Comprueba que la tarifa sea aplicable a la fecha indicada.</summary>
    public bool EstaVigente(DateTime fecha) => Importe >= 0 && fecha >= VigenteDesde &&
        (VigenteHasta is null || fecha < VigenteHasta.Value);
}
