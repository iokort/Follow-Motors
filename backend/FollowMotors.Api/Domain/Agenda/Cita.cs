namespace FollowMotors.Api.Domain.Agenda;

/// <summary>
/// Una cita es una atención del taller; puede corresponder a una motocicleta CFMOTO
/// registrada o a una moto externa. Se respeta la tabla "citas" de MySQL V3 de Gael.
/// Esta entidad NO administra clientes, contraseñas ni garantías (principio SRP).
/// </summary>
/// <remarks>
/// Las fechas se guardan como hora local de la sucursal, sin conversión a UTC, para
/// respetar la columna DATETIME y la zona America/Mexico_City del esquema existente.
/// </remarks>
public sealed record Cita(
    Guid Id,
    string TipoMotocicleta,
    Guid? MotocicletaId,
    string? MarcaExterna,
    string? ModeloExterno,
    string? ContactoExterno,
    string? TelefonoExterno,
    DateTime InicioLocal,
    int DuracionMinutos,
    string Motivo,
    string Estado,
    Guid RegistradaPor)
{
    /// <summary>Estados permitidos por la columna ENUM citas.estado de V3.</summary>
    public const string Programada = "PROGRAMADA";
    public const string Confirmada = "CONFIRMADA";
    public const string Cancelada = "CANCELADA";
    public const string Realizada = "REALIZADA";

    /// <summary>Conjunto de estados que admiten cambios de agenda.</summary>
    public bool SePuedeModificar => Estado is Programada or Confirmada;
}
