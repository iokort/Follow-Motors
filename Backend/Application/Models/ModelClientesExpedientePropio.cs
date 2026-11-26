namespace FollowMotors.Application.Models.Clientes;

using FollowMotors.Domain.Entities.Clientes;
using FollowMotors.Domain.Entities.Motocicletas;

/// <summary>Vista privada del propietario y sus motocicletas actuales.</summary>
public sealed record ExpedientePropio(Cliente Cliente, IReadOnlyList<Motocicleta> Motocicletas);

/// <summary>Datos de un expediente para uso del personal, excluidas las credenciales.</summary>
public sealed record ClienteFicha(ulong Id, ulong UsuarioId, string Nombres,
    string Apellidos, string Identificador, bool Activo, string? Telefono);
