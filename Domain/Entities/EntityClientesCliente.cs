namespace FollowMotors.Domain.Entities.Clientes;

/// <summary>Representa los datos personales del propietario de una motocicleta.</summary>
public sealed record Cliente(ulong Id, ulong UsuarioId, string Nombres, string Apellidos,
    string? Telefono, ulong? RegistradoPorUsuarioId, DateTime CreadoEn)
{
    /// <summary>Devuelve el nombre del cliente sin modificar sus datos originales.</summary>
    public string ObtenerNombreCompleto() => $"{Nombres} {Apellidos}".Trim();
}
