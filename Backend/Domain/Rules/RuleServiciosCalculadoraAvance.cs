namespace FollowMotors.Domain.Rules.Servicios;

/// <summary>Gobierna el plan ponderado y calcula el avance a partir del trabajo registrado.</summary>
public static class CalculadoraAvance
{
    public const decimal UmbralAviso = 25m;

    /// <summary>Exige un plan con ponderaciones representables que suman exactamente 100 %.</summary>
    public static bool PlanValido(IEnumerable<decimal> pesos)
    {
        var lista = pesos.ToArray();
        return lista.Length is > 0 and <= 100 && lista.All(p => p is > 0m and <= 100m && p == Math.Round(p, 2))
            && lista.Sum() == 100m;
    }

    /// <summary>Calcula el avance sin redondear antes de aplicar umbrales de negocio.</summary>
    /// <exception cref="ArgumentException">El plan o el progreso registrado no es coherente.</exception>
    public static decimal Calcular(IEnumerable<(decimal Peso, decimal Porcentaje)> tareas)
    {
        var lista = tareas.ToArray();
        if (!PlanValido(lista.Select(t => t.Peso)) || lista.Any(t => t.Porcentaje is < 0m or > 100m))
            throw new ArgumentException("Los pesos o los porcentajes de las tareas son inválidos.", nameof(tareas));
        return lista.Sum(t => t.Peso * t.Porcentaje) / 100m;
    }
}
