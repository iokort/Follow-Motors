namespace FollowMotors.Domain.Entities.Motocicletas;

/// <summary>Representa una motocicleta multimarca vinculada a un cliente.</summary>
public sealed record Motocicleta(ulong Id, ulong ClienteId, string NumeroSerie, string? Placa,
    string Marca, string Modelo, ushort? Anio, string? Color, uint KilometrajeActual, DateTime RegistradaEn)
{
    /// <summary>Comprueba si esta motocicleta pertenece al cliente indicado.</summary>
    public bool PerteneceA(ulong clienteId) => ClienteId == clienteId;
}
