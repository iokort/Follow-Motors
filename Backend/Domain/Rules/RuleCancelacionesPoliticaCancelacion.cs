namespace FollowMotors.Domain.Rules.Cancelaciones;

/// <summary>Establece el límite de cancelación de una reparación contratada.</summary>
public sealed class PoliticaCancelacion
{
    /// <summary>Permite cancelar un trabajo únicamente con avance verificable de hasta 60 %.</summary>
    public bool SePuedeCancelar(decimal avance, bool yaTerminada) =>
        !yaTerminada && avance is >= 0m and <= 60m;
}
