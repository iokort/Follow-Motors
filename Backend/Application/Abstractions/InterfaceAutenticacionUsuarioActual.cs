namespace FollowMotors.Application.Abstractions.Autenticacion;

/// <summary>Expone los datos mínimos del usuario autenticado al caso de uso.</summary>
public interface IUsuarioActual
{
    ulong? UsuarioId { get; }
    string Rol { get; }
}
