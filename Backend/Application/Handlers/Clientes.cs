namespace FollowMotors.Application.Handlers.Clientes;

using FollowMotors.Application.Abstractions.Autenticacion;
using FollowMotors.Application.Abstractions.Operaciones;
using FollowMotors.Application.Abstractions.Repositories.Clientes;
using FollowMotors.Application.Abstractions.Repositories.Motocicletas;
using FollowMotors.Application.Commands.Clientes;
using FollowMotors.Application.Models.Clientes;
using FollowMotors.Application.Queries.Clientes;
using MediatR;

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
