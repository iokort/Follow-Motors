namespace FollowMotors.Application.Handlers.Clientes;

using FollowMotors.Application.Abstractions.Autenticacion;
using FollowMotors.Application.Abstractions.Operaciones;
using FollowMotors.Application.Abstractions.Repositories.Clientes;
using FollowMotors.Application.Abstractions.Repositories.Motocicletas;
using FollowMotors.Application.Commands.Clientes;
using FollowMotors.Application.Models.Clientes;
using FollowMotors.Application.Queries.Clientes;
using MediatR;
using FollowMotors.Application.Operaciones;

/// <summary>Genera la consulta autorizada de los datos propios del cliente.</summary>
public sealed class ConsultarExpedientePropioHandler(IUsuarioActual usuario,
    IRepositorioClientes clientes, IRepositorioMotocicletas motos)
    : IRequestHandler<ConsultarExpedientePropioQuery, ExpedientePropio>
{
    /// <summary>Utiliza la identidad del token, nunca un clienteId enviado por el navegador.</summary>
    public async Task<ExpedientePropio> Handle(ConsultarExpedientePropioQuery request,
        CancellationToken cancellationToken)
    {
        if (usuario.Rol != "CLIENTE" || usuario.UsuarioId is null)
            throw new UnauthorizedAccessException("Se requiere una sesión de cliente.");
        var cliente = await clientes.ObtenerPorUsuarioAsync(usuario.UsuarioId.Value, cancellationToken)
            ?? throw new KeyNotFoundException("No existe el expediente del cliente.");
        var unidades = await motos.ListarPorClienteAsync(cliente.Id, cancellationToken);
        return new ExpedientePropio(cliente, unidades);
    }
}

/// <summary>Tramita altas presenciales sin persistencia directa.</summary>
public sealed class CrearCuentaClienteHandler(IRegistroCliente servicio)
    : IRequestHandler<CrearCuentaClienteCommand, ulong>
{
    /// <summary>Delega el registro a la interfaz del caso de uso.</summary>
    public Task<ulong> Handle(CrearCuentaClienteCommand request, CancellationToken cancellationToken) =>
        servicio.CrearAsync(request, cancellationToken);
}

/// <summary>Coordina la desactivación gerencial sin eliminar datos.</summary>
public sealed class DesactivarCuentaClienteHandler(IRegistroCliente servicio)
    : IRequestHandler<DesactivarCuentaClienteCommand, bool>
{
    /// <summary>Solicita la baja lógica a la interfaz de registro de clientes.</summary>
    public Task<bool> Handle(DesactivarCuentaClienteCommand request, CancellationToken cancellationToken) =>
        servicio.DesactivarAsync(request.ClienteId, cancellationToken);
}

/// <summary>Administra fichas de clientes sin mezclar cambios de contacto con credenciales.</summary>
public sealed class GestionExpedientesHandler(IRepositorioClientes clientes, IUsuarioActual usuario)
    : CasoUsoTaller(usuario), IRequestHandler<BuscarClientesQuery, IReadOnlyList<ClienteFicha>>,
      IRequestHandler<ObtenerClienteQuery, ClienteFicha>, IRequestHandler<ActualizarClienteCommand>
{
    /// <summary>Busca fichas por prefijo con un máximo de 50 resultados por página.</summary>
    public Task<IReadOnlyList<ClienteFicha>> Handle(BuscarClientesQuery solicitud, CancellationToken ct)
    {
        Empleado();
        if (solicitud.Pagina is < 1 or > 100_000 || solicitud.Buscar?.Length > 120)
            throw new ArgumentException("La página o el texto de búsqueda no es válido.");
        return clientes.BuscarAsync(solicitud.Buscar?.Trim(), solicitud.Pagina, ct);
    }

    /// <summary>Recupera el expediente del cliente indicado para uso interno.</summary>
    public async Task<ClienteFicha> Handle(ObtenerClienteQuery solicitud, CancellationToken ct)
    {
        Empleado();
        return await clientes.ObtenerFichaAsync(solicitud.Id, ct)
            ?? throw new KeyNotFoundException("No existe el cliente.");
    }

    /// <summary>Actualiza los datos de contacto y conserva el historial relacionado.</summary>
    public Task Handle(ActualizarClienteCommand solicitud, CancellationToken ct) =>
        clientes.ActualizarAsync(solicitud.Id, solicitud.Datos, Empleado().UsuarioId, ct);
}
