namespace FollowMotors.Application.Models.Clientes;

using FollowMotors.Domain.Entities.Clientes;
using FollowMotors.Domain.Entities.Motocicletas;

/// <summary>Vista privada del propietario y sus motocicletas actuales.</summary>
public sealed record ExpedientePropio(Cliente Cliente, IReadOnlyList<Motocicleta> Motocicletas);
