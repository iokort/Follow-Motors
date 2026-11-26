namespace FollowMotors.Application.Behaviors;

using FluentValidation;
using MediatR;

/// <summary>Ejecuta todos los validadores antes de entregar una solicitud a su Handler.</summary>
public sealed class ValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validadores)
    : IPipelineBehavior<TRequest, TResponse> where TRequest : notnull
{
    /// <summary>Reúne los errores y detiene solicitudes inválidas sin tocar la base de datos.</summary>
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var errores = new List<FluentValidation.Results.ValidationFailure>();
        foreach (var validador in validadores)
        {
            var resultado = await validador.ValidateAsync(new ValidationContext<TRequest>(request), cancellationToken);
            errores.AddRange(resultado.Errors);
        }
        if (errores.Count > 0) throw new ValidationException(errores);
        return await next();
    }
}
