using FollowMotors.Domain;

namespace FollowMotors.Application;

public sealed record IniciarSesionCommand(string Identificador, string Contrasena);
public sealed record SesionResponse(string Token, string Rol, long UsuarioId, DateTime VenceEnUtc);
public sealed record CrearClienteCommand(long UsuarioId, string Nombres, string Apellidos, string? Telefono);
public sealed record ActualizarClienteCommand(string Nombres, string Apellidos, string? Telefono);
public sealed record RegistrarMotocicletaCommand(long ClienteId, string NumeroSerie, string Marca, string Modelo, int? Anio, int KilometrajeActual);
public sealed record ActualizarMotocicletaCommand(string Marca, string Modelo, int? Anio, int KilometrajeActual);
public sealed record ReservarCitaCommand(long FranjaId, long ClienteId, long? MotocicletaId, string Motivo);
public sealed record ReprogramarCitaCommand(long FranjaId, string Motivo);
public sealed record RegistrarAtencionCommand(long ClienteId, long MotocicletaId, long? CitaId, long MecanicoId, string Motivo, int KilometrajeIngreso);
public sealed record ValidarServicioCommand(string Estado);
public sealed record CrearAvisoCommand(long ClienteId, long? CitaId, string Tipo, string Mensaje);

public interface IUsuarioQueries
{
    Task<Usuario?> BuscarPorIdentificadorAsync(string identificador, CancellationToken ct);
    Task<bool> ActivoAsync(long usuarioId, CancellationToken ct);
}
public interface IClientesStore
{
    Task<IReadOnlyList<Cliente>> ListarAsync(CancellationToken ct);
    Task<Cliente?> ObtenerAsync(long id, CancellationToken ct);
    Task<long?> BuscarIdPorUsuarioAsync(long usuarioId, CancellationToken ct);
    Task<long> CrearAsync(CrearClienteCommand comando, long operadorId, CancellationToken ct);
    Task<bool> ActualizarAsync(long id, ActualizarClienteCommand comando, CancellationToken ct);
}
public interface IMotocicletasStore
{
    Task<IReadOnlyList<Motocicleta>> ListarAsync(long? clienteId, CancellationToken ct);
    Task<long?> BuscarClienteAsync(long motoId, CancellationToken ct);
    Task<long> CrearAsync(RegistrarMotocicletaCommand comando, CancellationToken ct);
    Task<bool> ActualizarAsync(long id, ActualizarMotocicletaCommand comando, CancellationToken ct);
}
public interface ICitasStore
{
    Task<IReadOnlyList<Cita>> ListarAsync(long? clienteId, CancellationToken ct);
    Task<long> ReservarAsync(ReservarCitaCommand comando, string origen, CancellationToken ct);
    Task<bool> ReprogramarAsync(long id, ReprogramarCitaCommand comando, long? clienteId, CancellationToken ct);
    Task<bool> CancelarAsync(long id, long? clienteId, CancellationToken ct);
}
public sealed record AtencionResumen(long Id, long ClienteId, long MotocicletaId, string Estado, string Motivo, DateTime IngresadaEn);
public sealed record GarantiaResumen(long Id, long OrdenId, bool Aplica, DateTime? IniciaEn, DateTime? VenceEn, string Condiciones);
public interface IServiciosStore
{
    Task<IReadOnlyList<AtencionResumen>> ListarAsync(long? clienteId, CancellationToken ct);
    Task<long> RegistrarAsync(RegistrarAtencionCommand comando, CancellationToken ct);
    Task<bool> ValidarAsync(long id, ValidarServicioCommand comando, CancellationToken ct);
    Task<IReadOnlyList<GarantiaResumen>> GarantiasAsync(long? clienteId, CancellationToken ct);
}
public interface IReportesQueries
{
    Task<Indicadores> ConsultarIndicadoresAsync(CancellationToken ct);
}
public sealed record AvisoResumen(long Id, long ClienteId, string Tipo, string Mensaje, DateTime CreadaEn, DateTime? LeidaEn);
public interface IAvisosStore
{
    Task<IReadOnlyList<AvisoResumen>> ListarAsync(long clienteId, CancellationToken ct);
    Task<long> GenerarAsync(CrearAvisoCommand comando, CancellationToken ct);
}
public interface IProtectorContrasenas
{
    bool Verificar(string contrasena, string hash);
    string Generar(string contrasena);
}
public interface IGeneradorJwt
{
    SesionResponse Crear(Usuario usuario);
}
