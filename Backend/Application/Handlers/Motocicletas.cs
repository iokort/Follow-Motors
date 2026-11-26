namespace FollowMotors.Application.Handlers.Motocicletas;

using FollowMotors.Application.Abstractions.Autenticacion;
using FollowMotors.Application.Abstractions.Repositories.Clientes;
using FollowMotors.Application.Abstractions.Repositories.Motocicletas;
using FollowMotors.Application.Commands.Motocicletas;
using FollowMotors.Application.Queries.Motocicletas;
using FollowMotors.Domain.Entities.Motocicletas;
using FollowMotors.Application.Operaciones;
using MediatR;

/// <summary>Obtiene las motocicletas del propietario autenticado sin aceptar IDs arbitrarios.</summary>
public sealed class ConsultarMisMotocicletasHandler(IUsuarioActual usuario,
    IRepositorioClientes clientes, IRepositorioMotocicletas motocicletas)
    : IRequestHandler<ConsultarMisMotocicletasQuery, IReadOnlyList<Motocicleta>>
{
    /// <summary>Resuelve el cliente desde la identidad real y filtra por propietario.</summary>
    public async Task<IReadOnlyList<Motocicleta>> Handle(ConsultarMisMotocicletasQuery request,
        CancellationToken cancellationToken)
    {
        if (usuario.Rol != "CLIENTE" || usuario.UsuarioId is null)
            throw new UnauthorizedAccessException("Se requiere una sesión de cliente.");
        var cliente = await clientes.ObtenerPorUsuarioAsync(usuario.UsuarioId.Value, cancellationToken)
            ?? throw new KeyNotFoundException("No existe el expediente del cliente.");
        return await motocicletas.ListarPorClienteAsync(cliente.Id, cancellationToken);
    }
}

/// <summary>Controla el registro, actualización y consulta de fichas de motocicletas.</summary>
public sealed class FichasMotocicletasHandler(IRepositorioMotocicletas repositorio,
    IRepositorioClientes clientes, IUsuarioActual usuario) : CasoUsoTaller(usuario),
    IRequestHandler<RegistrarMotocicletaCommand, ulong>, IRequestHandler<ActualizarMotocicletaCommand>,
    IRequestHandler<ObtenerMotocicletaQuery, Motocicleta>,
    IRequestHandler<BuscarMotocicletasQuery, IReadOnlyList<Motocicleta>>
{
    /// <summary>Registra una unidad con VIN normalizado y propietario verificado.</summary>
    public Task<ulong> Handle(RegistrarMotocicletaCommand solicitud, CancellationToken ct) =>
        repositorio.RegistrarAsync(FormarFicha(0, solicitud), Empleado().UsuarioId, ct);

    /// <summary>Actualiza una ficha existente con las mismas reglas que el alta.</summary>
    public Task Handle(ActualizarMotocicletaCommand solicitud, CancellationToken ct) =>
        repositorio.ActualizarAsync(FormarFicha(solicitud.Id, solicitud.Datos), Empleado().UsuarioId, ct);

    /// <summary>Restringe el detalle para un cliente al titular de la unidad.</summary>
    public async Task<Motocicleta> Handle(ObtenerMotocicletaQuery solicitud, CancellationToken ct)
    {
        var actor = Actor();
        var moto = await repositorio.ObtenerPorIdAsync(solicitud.Id, ct);
        var propietario = actor.Rol == "CLIENTE" ? await clientes.ObtenerPorUsuarioAsync(actor.UsuarioId, ct) : null;
        if (moto is null || (actor.Rol == "CLIENTE" && (propietario is null || !moto.PerteneceA(propietario.Id))))
            throw new KeyNotFoundException("No existe la motocicleta.");
        return moto;
    }

    /// <summary>Busca fichas accesibles al personal con un máximo de 50 resultados por página.</summary>
    public Task<IReadOnlyList<Motocicleta>> Handle(BuscarMotocicletasQuery solicitud, CancellationToken ct)
    {
        Empleado();
        if (solicitud.Pagina is < 1 or > 100_000 || solicitud.Buscar?.Length > 120)
            throw new ArgumentException("La página o el texto de búsqueda no es válido.");
        return repositorio.BuscarAsync(solicitud.Buscar?.Trim(), solicitud.Pagina, ct);
    }

    /// <summary>Normaliza textos y convierte el año validado al tipo de la base de datos.</summary>
    private static Motocicleta FormarFicha(ulong id, RegistrarMotocicletaCommand datos) =>
        new(id, datos.ClienteId, datos.NumeroSerie.Trim().ToUpperInvariant(), datos.Placa?.Trim(),
            datos.Marca.Trim(), datos.Modelo.Trim(), (ushort?)datos.Anio, datos.Color?.Trim(),
            datos.KilometrajeActual, default);
}
