namespace FollowMotors.Application.Handlers.Clientes;

using MediatR;
using FollowMotors.Application.Abstractions.Autenticacion;
using FollowMotors.Application.Abstractions.Repositories.Clientes;
using FollowMotors.Application.Abstractions.Repositories.Motocicletas;
using FollowMotors.Application.Models.Clientes;
using FollowMotors.Application.Queries.Clientes;

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
