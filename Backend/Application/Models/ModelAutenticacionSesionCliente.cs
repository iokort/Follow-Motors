namespace FollowMotors.Application.Models.Autenticacion;

/// <summary>Resultado del acceso a la API, sin revelar contraseña ni hash.</summary>
public sealed record SesionCliente(ulong UsuarioId, string Identificador, string Rol,
    string Token, DateTime ExpiraEnUtc);
