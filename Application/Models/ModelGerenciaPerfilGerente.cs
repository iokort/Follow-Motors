namespace FollowMotors.Application.Models.Gerencia;

/// <summary>Representa la información pública del perfil gerencial, sin credenciales.</summary>
public sealed record PerfilGerente(ulong Id, string Nombres, string Apellidos, bool Disponible);
