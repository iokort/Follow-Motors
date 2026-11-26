namespace FollowMotors.Domain.Rules.Seguridad;

/// <summary>Contrato de permisos del negocio, independiente de HTTP.</summary>
public interface IPoliticaPermisos
{
    /// <summary>Autoriza el alta presencial de clientes.</summary>
    bool PuedeRegistrarClientes(string rol);
    /// <summary>Autoriza bajas lógicas de cuentas cliente.</summary>
    bool PuedeDesactivarCliente(string rol);
    /// <summary>Autoriza la administración del personal.</summary>
    bool PuedeAdministrarPersonal(string rol);
    /// <summary>Autoriza operaciones del ingreso y revisión.</summary>
    bool PuedeGestionarAtenciones(string rol);
    /// <summary>Comprueba si un empleado puede registrar diagnósticos.</summary>
    bool PuedeDiagnosticar(string rol, ulong? empleadoId, ulong mecanicoAsignadoId);
    /// <summary>Autoriza la consulta de un expediente según su propietario.</summary>
    bool PuedeConsultarPropiedad(string rol, ulong? clienteActualId, ulong propietarioId);
}

/// <summary>Centraliza las decisiones de rol que protegen operaciones de negocio.</summary>
public sealed class PoliticaPermisos : IPoliticaPermisos
{
    /// <summary>Determina si la cuenta puede registrar clientes presencialmente.</summary>
    public bool PuedeRegistrarClientes(string rol) => rol is "GERENTE" or "EMPLEADO";

    /// <summary>Determina si la cuenta puede desactivar a un cliente.</summary>
    public bool PuedeDesactivarCliente(string rol) => rol == "GERENTE";

    /// <summary>Determina si puede registrar personal o consultar auditoría.</summary>
    public bool PuedeAdministrarPersonal(string rol) => rol == "GERENTE";

    /// <summary>Determina si puede recibir motocicletas y cobrar revisiones.</summary>
    public bool PuedeGestionarAtenciones(string rol) => rol is "GERENTE" or "EMPLEADO";

    /// <summary>Determina si una persona puede registrar un diagnóstico asignado.</summary>
    public bool PuedeDiagnosticar(string rol, ulong? empleadoId, ulong mecanicoAsignadoId) =>
        PuedeRegistrarTrabajo(rol, empleadoId, mecanicoAsignadoId);

    /// <summary>Autoriza trabajo técnico a gerencia o al responsable que identifica la operación.</summary>
    public static bool PuedeRegistrarTrabajo(string rol, ulong? persona, ulong responsable) =>
        rol == "GERENTE" || (rol == "EMPLEADO" && persona == responsable);

    /// <summary>Evita mostrar a un cliente información de una moto ajena.</summary>
    public bool PuedeConsultarPropiedad(string rol, ulong? clienteActualId, ulong propietarioId) =>
        rol is "GERENTE" or "EMPLEADO" || (rol == "CLIENTE" && clienteActualId == propietarioId);
}
