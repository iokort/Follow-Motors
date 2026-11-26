using FollowMotors.Domain;

namespace FollowMotors.Application;

// CQRS: las órdenes alteran estado, mientras las consultas no realizan escrituras.
public sealed class LoginHandler(IUsuarioQueries usuarios, IProtectorContrasenas contrasenas, IGeneradorJwt jwt)
{
    public async Task<SesionResponse?> Handle(IniciarSesionCommand comando, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(comando.Identificador) || string.IsNullOrWhiteSpace(comando.Contrasena))
            return null;
        var usuario = await usuarios.BuscarPorIdentificadorAsync(comando.Identificador.Trim(), ct);
        if (usuario is null || !usuario.Activo || !contrasenas.Verificar(comando.Contrasena, usuario.HashContrasena))
            return null;
        return jwt.Crear(usuario);
    }
}
public sealed class ClienteCommandHandler(IClientesStore clientes)
{
    public Task<long> CrearAsync(CrearClienteCommand comando, long operador, CancellationToken ct)
    {
        if (comando.UsuarioId <= 0 || string.IsNullOrWhiteSpace(comando.Nombres) || string.IsNullOrWhiteSpace(comando.Apellidos))
            throw new ArgumentException("Usuario, nombres y apellidos son obligatorios.");
        return clientes.CrearAsync(comando, operador, ct);
    }
    public Task<bool> ActualizarAsync(long id, ActualizarClienteCommand comando, CancellationToken ct)
    {
        if (id <= 0 || string.IsNullOrWhiteSpace(comando.Nombres) || string.IsNullOrWhiteSpace(comando.Apellidos))
            throw new ArgumentException("Datos del cliente no válidos.");
        return clientes.ActualizarAsync(id, comando, ct);
    }
}
public sealed class MotocicletaCommandHandler(IMotocicletasStore motos)
{
    public Task<long> RegistrarAsync(RegistrarMotocicletaCommand comando, CancellationToken ct)
    {
        if (comando.ClienteId <= 0 || string.IsNullOrWhiteSpace(comando.NumeroSerie) ||
            string.IsNullOrWhiteSpace(comando.Marca) || string.IsNullOrWhiteSpace(comando.Modelo) ||
            comando.KilometrajeActual < 0 || comando.Anio is < 1900 or > 2150)
            throw new ArgumentException("Datos de motocicleta no válidos.");
        return motos.CrearAsync(comando, ct);
    }
}
public sealed class CitaCommandHandler(ICitasStore citas, IMotocicletasStore motos)
{
    public async Task<long> ReservarAsync(ReservarCitaCommand comando, string origen, CancellationToken ct)
    {
        if (comando.ClienteId <= 0 || comando.FranjaId <= 0 || string.IsNullOrWhiteSpace(comando.Motivo))
            throw new ArgumentException("Franja, cliente y motivo son obligatorios.");
        if (comando.MotocicletaId is long motoId && await motos.BuscarClienteAsync(motoId, ct) != comando.ClienteId)
            throw new UnauthorizedAccessException("La motocicleta no pertenece al cliente.");
        return await citas.ReservarAsync(comando, origen, ct);
    }
}
public sealed class ServicioCommandHandler(IServiciosStore servicios, IMotocicletasStore motos)
{
    public async Task<long> RegistrarAsync(RegistrarAtencionCommand comando, CancellationToken ct)
    {
        if (comando.ClienteId <= 0 || comando.MotocicletaId <= 0 || comando.MecanicoId <= 0 ||
            comando.KilometrajeIngreso < 0 || string.IsNullOrWhiteSpace(comando.Motivo))
            throw new ArgumentException("Datos de recepción incompletos.");
        if (await motos.BuscarClienteAsync(comando.MotocicletaId, ct) != comando.ClienteId)
            throw new ArgumentException("La motocicleta no pertenece al cliente.");
        return await servicios.RegistrarAsync(comando, ct);
    }
}
