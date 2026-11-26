namespace FollowMotors.Domain.Rules.Servicios;

/// <summary>Calcula el avance real ponderado de tareas verificadas.</summary>
public sealed class CalculadoraAvance
{
    /// <summary>Suma el peso de cada tarea multiplicado por su fracción ejecutada.</summary>
    public decimal Calcular(IEnumerable<(decimal Peso, decimal Fraccion)> tareas)
    {
        var lista = tareas.ToArray();
        if (lista.Any(t => t.Peso <= 0 || t.Fraccion is < 0m or > 1m))
            throw new ArgumentException("Las ponderaciones o fracciones son inválidas.", nameof(tareas));
        if (lista.Length > 0 && lista.Sum(t => t.Peso) != 100m)
            throw new ArgumentException("Los pesos de tareas deben sumar 100 %.", nameof(tareas));
        return Math.Clamp(lista.Sum(t => t.Peso * t.Fraccion), 0m, 100m);
    }
}
