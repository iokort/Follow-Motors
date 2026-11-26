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
