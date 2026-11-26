namespace FollowMotors.Application.Abstractions.Autenticacion;

/// <summary>Emite un token firmado para una identidad autenticada.</summary>
public interface IGeneradorTokens
{
    /// <summary>Genera el token sin introducir lógica de transporte en Application.</summary>
    string Generar(ulong usuarioId, string identificador, string rol);
}
