namespace FollowMotors.Application.Common.Excepciones;

/// <summary>Indica un fallo de credenciales sin revelar si existe la cuenta.</summary>
public sealed class CredencialesInvalidasException : Exception
{
    /// <summary>Construye un error controlado de autenticación.</summary>
    public CredencialesInvalidasException() : base("Identificador o contraseña incorrectos.") { }
}
