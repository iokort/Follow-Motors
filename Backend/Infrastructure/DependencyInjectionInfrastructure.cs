namespace FollowMotors.Infrastructure;

using Microsoft.Extensions.DependencyInjection;
using FollowMotors.Application.Operaciones;
using FollowMotors.Infrastructure.Operaciones;
using FollowMotors.Application.Abstractions.Autenticacion;
using FollowMotors.Application.Abstractions.Repositories.Atenciones;
using FollowMotors.Application.Abstractions.Repositories.Auditoria;
using FollowMotors.Application.Abstractions.Repositories.Clientes;
using FollowMotors.Application.Abstractions.Repositories.Motocicletas;
using FollowMotors.Application.Abstractions.Repositories.Personal;
using FollowMotors.Infrastructure.Autenticacion;
using FollowMotors.Infrastructure.Database;
using FollowMotors.Infrastructure.Repositories.Atenciones;
using FollowMotors.Infrastructure.Repositories.Auditoria;
using FollowMotors.Infrastructure.Repositories.Clientes;
using FollowMotors.Infrastructure.Repositories.Motocicletas;
using FollowMotors.Infrastructure.Repositories.Personal;

/// <summary>Registra las implementaciones concretas sin exponer MySQL a Application.</summary>
public static class DependencyInjection
{
    /// <summary>Configura las conexiones y los repositorios que dependen de infraestructura.</summary>
    public static IServiceCollection AddInfrastructure(this IServiceCollection servicios, string conexion)
    {
        if (string.IsNullOrWhiteSpace(conexion))
            throw new InvalidOperationException("Configure ConnectionStrings:MySql por variable de entorno.");
        servicios.AddSingleton(new ConexionMySql(conexion));
        servicios.AddScoped<IOperacionesTaller, OperacionesTallerMySql>();
        servicios.AddScoped<IRepositorioClientes, RepositorioClientes>();
        servicios.AddScoped<IRepositorioCuentas, RepositorioCuentas>();
        servicios.AddScoped<IRepositorioMotocicletas, RepositorioMotocicletas>();
        servicios.AddScoped<IRepositorioAtenciones, RepositorioAtenciones>();
        servicios.AddScoped<IRepositorioPersonal, RepositorioPersonal>();
        servicios.AddScoped<IRepositorioAuditoria, RepositorioAuditoria>();
        servicios.AddScoped<IProtectorContrasenas, ProtectorContrasenas>();
        servicios.AddScoped<IServicioAutenticacion, ServicioAutenticacion>();
        return servicios;
    }
}
