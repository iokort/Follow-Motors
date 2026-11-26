namespace FollowMotors.Application;

using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using FollowMotors.Application.Abstractions.Operaciones;
using FollowMotors.Application.Behaviors;
using FollowMotors.Application.Services.Clientes;
using FollowMotors.Application.Services.Atenciones;
using FollowMotors.Domain.Rules.Atenciones;
using FollowMotors.Domain.Rules.Seguridad;

/// <summary>Registra MediatR, validadores, políticas y casos de uso de Application.</summary>
public static class DependencyInjection
{
    /// <summary>Configura las dependencias de Application sin referencias a Infrastructure.</summary>
    public static IServiceCollection AddApplication(this IServiceCollection servicios)
    {
        servicios.AddMediatR(configuracion =>
        {
            configuracion.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly);
            configuracion.AddOpenBehavior(typeof(ValidationBehavior<,>));
        });
        servicios.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);
        servicios.AddScoped<IRegistroCliente, RegistroCliente>();
        servicios.AddScoped<IRecepcionTaller, RecepcionTaller>();
        servicios.AddSingleton<IPoliticaPermisos, PoliticaPermisos>();
        servicios.AddSingleton<IPoliticaAtenciones, PoliticaAtenciones>();
        return servicios;
    }
}
