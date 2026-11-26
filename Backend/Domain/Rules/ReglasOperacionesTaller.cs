namespace FollowMotors.Domain.Rules.Operaciones;

/// <summary>Reglas puras del taller: compartidas por los casos de uso sin acceso a SQL.</summary>
public static class ReglasOperacionesTaller
{
    /// <summary>Verifica montos no negativos, descripción y ponderación total de tareas.</summary>
    public static void ValidarPresupuesto(string descripcion, decimal manoObra, decimal refacciones,
        IEnumerable<decimal> pesos)
    {
        if (string.IsNullOrWhiteSpace(descripcion) || descripcion.Length > 4000)
            throw new ArgumentException("La descripción del trabajo es obligatoria.");
        if (manoObra < 0 || refacciones < 0 || manoObra + refacciones <= 0)
            throw new ArgumentException("El presupuesto debe tener un importe positivo.");
        var lista = pesos.ToArray();
        if (lista.Length == 0 || lista.Length > 100 || lista.Any(p => p <= 0 || p > 100)
            || lista.Sum() != 100m)
            throw new ArgumentException("Los pesos de las tareas deben sumar exactamente 100 %.");
    }

    /// <summary>Valida un movimiento positivo de refacciones; la disponibilidad se verifica en MySQL.</summary>
    public static void ValidarCantidad(int cantidad)
    {
        if (cantidad <= 0) throw new ArgumentException("La cantidad debe ser mayor que cero.");
    }

    /// <summary>Valida el rango porcentual antes de actualizar una tarea.</summary>
    public static void ValidarPorcentaje(decimal porcentaje)
    {
        if (porcentaje < 0 || porcentaje > 100)
            throw new ArgumentException("El avance debe estar entre 0 y 100 %.");
    }

    /// <summary>Evita importes de anticipo negativos o completamente vacíos.</summary>
    public static void ValidarAnticipo(decimal manoObra, decimal refacciones)
    {
        if (manoObra < 0 || refacciones < 0 || manoObra + refacciones <= 0)
            throw new ArgumentException("El anticipo debe ser positivo.");
    }

    /// <summary>Calcula el reintegro de lo abonado menos trabajo ejecutado y material usado.</summary>
    public static decimal CalcularReembolso(decimal abonado, decimal trabajoRealizado, decimal materialUtilizado) =>
        Math.Max(0m, abonado - Math.Max(0m, trabajoRealizado) - Math.Max(0m, materialUtilizado));
}
