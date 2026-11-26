namespace FollowMotors.Domain;

public sealed record Usuario(long Id, string Identificador, string HashContrasena, string Rol, bool Activo);
public sealed record Cliente(long Id, long UsuarioId, string Nombres, string Apellidos, string? Telefono);
public sealed record Motocicleta(long Id, long ClienteId, string NumeroSerie, string Marca, string Modelo, int? Anio, int KilometrajeActual);
public sealed record Cita(long Id, long FranjaId, long ClienteId, long? MotocicletaId, string Motivo, string Estado, DateTime Inicio);
public sealed record Indicadores(int Clientes, int Motocicletas, int CitasPendientes, int ServiciosEnProceso);
public sealed record RegistroRecordatorio(long Id, long ClienteId, string Canal, string Mensaje, DateTime ProgramadoPara, string Estado);
public static class Roles
{
    public const string Gerente = "GERENTE";
    public const string Empleado = "EMPLEADO";
    public const string Cliente = "CLIENTE";
}
