namespace FollowMotors.Tests;

using FollowMotors.Domain.Rules.Agenda;
using FollowMotors.Domain.Rules.Atenciones;
using FollowMotors.Domain.Rules.Cancelaciones;
using FollowMotors.Domain.Rules.Operaciones;
using FollowMotors.Domain.Rules.Seguridad;
using FollowMotors.Domain.Rules.Servicios;
using Xunit;

/// <summary>Casos unitarios de dominio, independientes de HTTP y MySQL.</summary>
public sealed class ReglasDelTaller
{
    /// <summary>Exige ponderaciones positivas y un total exacto y representable.</summary>
    [Fact]
    public void PlanValidoRequierePesosExactos()
    {
        Assert.True(CalculadoraAvance.PlanValido([66.67m, 33.33m]));
        Assert.False(CalculadoraAvance.PlanValido([60m, 30m]));
        Assert.False(CalculadoraAvance.PlanValido([60m, 50m]));
        Assert.False(CalculadoraAvance.PlanValido([99.999m, 0.001m]));
        Assert.False(CalculadoraAvance.PlanValido([]));
    }

    /// <summary>Conserva el avance ponderado que distingue el umbral de 60 % de sus fracciones.</summary>
    [Fact]
    public void ElAvanceUsaTrabajoPonderado()
    {
        Assert.Equal(24m, CalculadoraAvance.Calcular([(60m, 40m), (40m, 0m)]));
        Assert.Equal(60.01m, CalculadoraAvance.Calcular([(50m, 100m), (50m, 20.02m)]));
        Assert.Equal(60.004m, CalculadoraAvance.Calcular([(60m, 100m), (40m, 0.01m)]));
        Assert.Throws<ArgumentException>(() => CalculadoraAvance.Calcular([(100m, 101m)]));
    }

    /// <summary>Comprueba los valores directamente a ambos lados de los límites.</summary>
    [Theory]
    [InlineData("0", true)]
    [InlineData("60", true)]
    [InlineData("60.004", false)]
    [InlineData("60.01", false)]
    [InlineData("-0.01", false)]
    public void LimiteDeCancelacion(string avance, bool esperado) =>
        Assert.Equal(esperado, PoliticaCancelacion.AvanceCancelable(
            decimal.Parse(avance, System.Globalization.CultureInfo.InvariantCulture)));

    /// <summary>Calcula retención por trabajo y piezas sin considerar la tarifa de diagnóstico.</summary>
    [Fact]
    public void LiquidacionUsaSoloElAnticipo()
    {
        Assert.Equal(new LiquidacionCancelacion(600m, 80m, 560m), PoliticaCancelacion.Liquidar(1240m, 1000m, 60m, 80m));
        Assert.Equal(1240m, PoliticaCancelacion.Liquidar(1240m, 1000m, 0m, 0m).Reembolso);
        Assert.Equal(0m, PoliticaCancelacion.Liquidar(10m, 100m, 60m, 40m).Reembolso);
        Assert.Throws<ArgumentOutOfRangeException>(() => PoliticaCancelacion.Liquidar(1240m, 1000m, 60.01m, 80m));
    }

    /// <summary>Impide avanzar sin anticipo o mientras la reparación espera material.</summary>
    [Fact]
    public void EjecucionNecesitaPagoYPiezas()
    {
        Assert.False(EstadoReparacion.PermiteEjecutar("LISTA", null));
        Assert.False(EstadoReparacion.PermiteEjecutar("PENDIENTE_PIEZAS", 500m));
        Assert.False(EstadoReparacion.PermiteEjecutar("CANCELADA", 500m));
        Assert.True(EstadoReparacion.PermiteEjecutar("EN_EJECUCION", 500m));
    }

    /// <summary>Conserva la capacidad y el horario mientras haya citas confirmadas.</summary>
    [Fact]
    public void CapacidadEsEstrictoParaElUltimoCupo()
    {
        Assert.True(ControlCapacidad.HayDisponibilidad(2, 1));
        Assert.False(ControlCapacidad.HayDisponibilidad(2, 2));
        Assert.Throws<InvalidOperationException>(() => ControlCapacidad.ValidarCambio(1, 2, true));
        Assert.Throws<InvalidOperationException>(() => ControlCapacidad.ValidarCambio(2, 1, false));
        ControlCapacidad.ValidarCambio(2, 0, false);
    }

    /// <summary>Desempata por identificador y rechaza asignar si la plantilla disponible está vacía.</summary>
    [Fact]
    public void SeleccionaElMecanicoDeMenorCarga()
    {
        Assert.Equal(3UL, AsignacionMecanicos.Elegir([new(2, 5), new(5, 2), new(3, 2)]));
        Assert.Throws<InvalidOperationException>(() => AsignacionMecanicos.Elegir([]));
    }

    /// <summary>Verifica que solo gerencia y el mecánico responsable registren trabajo técnico.</summary>
    [Fact]
    public void TrabajoTecnicoRequiereResponsable()
    {
        Assert.False(PoliticaPermisos.PuedeRegistrarTrabajo("CLIENTE", 4, 4));
        Assert.False(PoliticaPermisos.PuedeRegistrarTrabajo("EMPLEADO", 4, 5));
        Assert.True(PoliticaPermisos.PuedeRegistrarTrabajo("EMPLEADO", 4, 4));
        Assert.True(PoliticaPermisos.PuedeRegistrarTrabajo("GERENTE", 4, 5));
    }

    /// <summary>Controla la activación tras la entrega y el límite de vigencia.</summary>
    [Fact]
    public void VigenciaDependeDeEntregaYFechas()
    {
        var inicio = new DateTime(2026, 11, 26, 10, 0, 0, DateTimeKind.Utc);
        var vence = inicio.AddDays(30);
        Assert.Equal("INFORMADA", VigenciaGarantia.Estado(true, "TERMINADA", null, null, inicio));
        Assert.Equal("SIN_COBERTURA", VigenciaGarantia.Estado(true, "CANCELADA", null, null, inicio));
        Assert.Equal("SIN_COBERTURA", VigenciaGarantia.Estado(false, "ENTREGADA", null, null, inicio));
        Assert.Equal("VIGENTE", VigenciaGarantia.Estado(true, "ENTREGADA", inicio, vence, inicio));
        Assert.Equal("VENCIDA", VigenciaGarantia.Estado(true, "ENTREGADA", inicio, vence, vence));
    }

    /// <summary>Rechaza importes que sobrepasan la precisión del esquema.</summary>
    [Fact]
    public void TotalesRespetanRangoMonetario()
    {
        Assert.True(ImportesReparacion.EsTotalValido(1000m, 240m));
        Assert.False(ImportesReparacion.EsTotalValido(0m, 0m));
        Assert.False(ImportesReparacion.EsTotalValido(9_999_999_999m, 1m));
        Assert.False(ImportesReparacion.EsTotalValido(decimal.MaxValue, decimal.MaxValue));
    }
}
