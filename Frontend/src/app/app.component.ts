import { CommonModule } from '@angular/common';
import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ApiService, mensajeError } from './api.service';
import {
  AccesoDemo, Atencion, Auditoria, Cliente, EstadoApi, Expediente, Flujo,
  Motocicleta, PerfilGerente, Personal, ResumenDemo, Sesion
} from './models';

type Vista = 'inicio' | 'clientes' | 'personal' | 'motocicletas' |
  'atenciones' | 'auditoria' | 'perfil' | 'expediente';

/** Interfaz de demostración de los casos de uso implementados en el backend C#. */
@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './app.component.html',
  styleUrl: './app.component.css'
})
export class AppComponent implements OnInit {
  readonly api = inject(ApiService);
  readonly vista = signal<Vista>('inicio');
  readonly ocupado = signal(false);
  readonly enLinea = signal<boolean | null>(null);
  readonly mensaje = signal('');
  readonly esError = signal(false);
  readonly datos = signal<ResumenDemo | null>(null);
  readonly cuentasDemo = signal<AccesoDemo[]>([]);
  readonly miExpediente = signal<Expediente | null>(null);
  readonly perfil = signal<PerfilGerente | null>(null);
  readonly auditorias = signal<Auditoria[]>([]);
  readonly atencionConsultada = signal<Atencion | null>(null);
  readonly flujoActivo = signal<Flujo | null>(null);
  readonly sesion = computed(() => this.api.sesion());
  readonly esGerente = computed(() => this.sesion()?.rol === 'GERENTE');
  readonly esPersonal = computed(() => this.sesion()?.rol === 'GERENTE' || this.sesion()?.rol === 'EMPLEADO');
  readonly bienvenida = computed(() => this.sesion()?.identificador.split('@')[0] || 'Visitante');
  readonly mecanicos = computed(() => this.datos()?.personal.filter(x => x.esMecanico && x.disponible) ?? []);
  readonly clientesActivos = computed(() => this.datos()?.clientes.filter(x => x.activo) ?? []);
  readonly atencionesRecientes = computed(() => this.datos()?.atenciones.slice(0, 5) ?? []);

  login = { identificador: '', contrasena: '' };
  nuevoCliente = { identificador: '', contrasena: '', nombres: '', apellidos: '', telefono: '' };
  nuevoEmpleado = { identificador: '', contrasena: '', nombres: '', apellidos: '', esMecanico: false };
  nuevaMoto = { clienteId: 1, numeroSerie: '', marca: '', modelo: '', anio: 2024,
    placa: '', color: '', kilometrajeActual: 0 };
  nuevaAtencion = { clienteId: 1, motocicletaId: 1, mecanicoId: 3, motivo: '',
    kilometrajeIngreso: 0, citaId: null as number | null };
  diagnostico = { atencionId: 1, hallazgos: '' };
  cobro = { atencionId: 1, referenciaComprobante: '' };
  consultarId = 1;

  readonly flujos: Record<string, Flujo> = {
    login: {
      titulo: 'Autenticación y autorización', objetivo: 'Login real con JWT, roles y contraseña protegida.',
      ruta: 'POST /api/autenticacion/iniciar-sesion',
      archivos: ['ControllerAutenticacion.cs', 'CommandAutenticacionIniciarSesion.cs',
        'ValidatorAutenticacionIniciarSesion.cs', 'BehaviorValidation.cs',
        'HandlerAutenticacionIniciarSesion.cs', 'InterfaceAutenticacionServicioAutenticacion.cs',
        'AuthAutenticacion.cs', 'InterfaceClientesRepositorioCuentas.cs', 'DemoDatosEnMemoria.cs',
        'AuthGeneradorTokensJwt.cs']
    },
    gerente: {
      titulo: 'Perfil del gerente', objetivo: 'Consulta protegida con rol GERENTE.',
      ruta: 'GET /api/gerencia/mi-perfil',
      archivos: ['ControllerGerencia.cs', 'QueryGerenciaConsultarPerfilGerente.cs',
        'HandlerGerenciaConsultarPerfilGerente.cs', 'InterfacePersonalRepositorioPersonal.cs',
        'DemoDatosEnMemoria.cs', 'EntityPersonalGerente.cs']
    },
    clienteCrear: {
      titulo: 'Alta presencial de cliente', objetivo: 'US01 · Cuenta, expediente y auditoría sin registro público.',
      ruta: 'POST /api/clientes',
      archivos: ['ControllerClientes.cs', 'CommandClientesCrearCuentaCliente.cs',
        'ValidatorClientesCrearCuentaCliente.cs', 'BehaviorValidation.cs',
        'HandlerClientesCrearCuentaCliente.cs', 'InterfaceClientesRegistroCliente.cs',
        'ServiceClientesRegistroCliente.cs', 'InterfaceClientesRepositorioClientes.cs',
        'DemoDatosEnMemoria.cs', 'EntityClientesCliente.cs']
    },
    clienteBaja: {
      titulo: 'Desactivar cliente', objetivo: 'US01 · Solo GERENTE; se preserva el historial.',
      ruta: 'DELETE /api/clientes/{id}',
      archivos: ['ControllerClientes.cs', 'CommandClientesDesactivarCuentaCliente.cs',
        'ValidatorClientesDesactivarCuentaCliente.cs', 'HandlerClientesDesactivarCuentaCliente.cs',
        'ServiceClientesRegistroCliente.cs', 'RuleSeguridadPoliticaPermisos.cs', 'DemoDatosEnMemoria.cs']
    },
    empleado: {
      titulo: 'Alta del personal', objetivo: 'Gerencia registra empleados y mecánicos sin acceder a SQL.',
      ruta: 'POST /api/personal',
      archivos: ['ControllerPersonal.cs', 'CommandPersonalRegistrarEmpleado.cs',
        'ValidatorPersonalRegistrarEmpleado.cs', 'HandlerPersonalRegistrarEmpleado.cs',
        'InterfacePersonalRepositorioPersonal.cs', 'DemoDatosEnMemoria.cs', 'EntityPersonalEmpleado.cs']
    },
    personal: {
      titulo: 'Consulta de personal', objetivo: 'Gerencia consulta perfiles mediante un Query y un Handler.',
      ruta: 'GET /api/personal',
      archivos: ['ControllerPersonal.cs', 'QueryPersonalConsultarPersonal.cs',
        'HandlerPersonalConsultarPersonal.cs', 'InterfacePersonalRepositorioPersonal.cs',
        'DemoDatosEnMemoria.cs']
    },
    moto: {
      titulo: 'Registrar motocicleta', objetivo: 'US02 · Vincular unidad multimarca, VIN único y propiedad.',
      ruta: 'POST /api/motocicletas',
      archivos: ['ControllerMotocicletas.cs', 'CommandMotocicletasRegistrarMotocicleta.cs',
        'ValidatorMotocicletasRegistrarMotocicleta.cs', 'HandlerMotocicletasRegistrarMotocicleta.cs',
        'InterfaceMotocicletasRepositorioMotocicletas.cs', 'DemoDatosEnMemoria.cs', 'EntityMotocicletasMotocicleta.cs']
    },
    motosPropias: {
      titulo: 'Consulta de motocicletas propias', objetivo: 'US03 · El cliente no puede consultar registros ajenos.',
      ruta: 'GET /api/motocicletas/mis-motocicletas',
      archivos: ['ControllerMotocicletas.cs', 'QueryMotocicletasConsultarMisMotocicletas.cs',
        'HandlerMotocicletasConsultarMisMotocicletas.cs', 'DemoDatosEnMemoria.cs']
    },
    expediente: {
      titulo: 'Expediente privado', objetivo: 'US03 · El JWT identifica al propietario y filtra su expediente.',
      ruta: 'GET /api/clientes/mi-expediente',
      archivos: ['ControllerClientes.cs', 'QueryClientesConsultarExpedientePropio.cs',
        'HandlerClientesConsultarExpedientePropio.cs', 'InterfaceClientesRepositorioClientes.cs',
        'InterfaceMotocicletasRepositorioMotocicletas.cs', 'DemoDatosEnMemoria.cs']
    },
    recepcion: {
      titulo: 'Recepción de motocicleta', objetivo: 'US06 · Crear atención y vincular mecánico responsable.',
      ruta: 'POST /api/atenciones',
      archivos: ['ControllerAtenciones.cs', 'CommandAtencionesRecepcionarMotocicleta.cs',
        'ValidatorAtencionesRecepcionarMotocicleta.cs', 'HandlerAtencionesRecepcionarMotocicleta.cs',
        'ServiceAtencionesRecepcionTaller.cs', 'InterfaceAtencionesRepositorioAtenciones.cs',
        'DemoDatosEnMemoria.cs', 'EntityAtencionesAtencionTaller.cs']
    },
    diagnostico: {
      titulo: 'Registro de diagnóstico', objetivo: 'US06 · Solo el mecánico asignado o el gerente.',
      ruta: 'POST /api/atenciones/{id}/diagnostico',
      archivos: ['ControllerAtenciones.cs', 'CommandAtencionesRegistrarDiagnostico.cs',
        'ValidatorAtencionesRegistrarDiagnostico.cs', 'HandlerAtencionesRegistrarDiagnostico.cs',
        'ServiceAtencionesRecepcionTaller.cs', 'RuleAtencionesPoliticaAtenciones.cs',
        'DemoDatosEnMemoria.cs']
    },
    cobro: {
      titulo: 'Cobro de revisión', objetivo: 'US06 · Tarifa fija y prevención de cobros duplicados.',
      ruta: 'POST /api/atenciones/{id}/cobro',
      archivos: ['ControllerAtenciones.cs', 'CommandAtencionesCobrarRevision.cs',
        'ValidatorAtencionesCobrarRevision.cs', 'HandlerAtencionesCobrarRevision.cs',
        'ServiceAtencionesRecepcionTaller.cs', 'DemoDatosEnMemoria.cs']
    },
    atencion: {
      titulo: 'Consulta de una atención', objetivo: 'US06 · Lectura autorizada por rol y propiedad.',
      ruta: 'GET /api/atenciones/{id}',
      archivos: ['ControllerAtenciones.cs', 'QueryAtencionesConsultarAtencion.cs',
        'ValidatorAtencionesConsultarAtencion.cs', 'HandlerAtencionesConsultarAtencion.cs',
        'RuleSeguridadPoliticaPermisos.cs', 'DemoDatosEnMemoria.cs']
    },
    auditoria: {
      titulo: 'Auditoría de operaciones', objetivo: 'Consulta exclusiva de gerente; muestra quién cambió qué.',
      ruta: 'GET /api/auditoria?limite=50',
      archivos: ['ControllerAuditoria.cs', 'QueryAuditoriaConsultarAuditoria.cs',
        'HandlerAuditoriaConsultarAuditoria.cs', 'InterfaceAuditoriaRepositorioAuditoria.cs',
        'DemoDatosEnMemoria.cs', 'EntityAuditoriaRegistroAuditoria.cs']
    }
  };

  /** Comprueba el estado del servidor y consulta los accesos ficticios de demo. */
  ngOnInit(): void { void this.inicializar(); }

  /** Prepara el estado inicial de la aplicación. */
  private async inicializar(): Promise<void> {
    try {
      const estado = await this.api.solicitar<EstadoApi>('GET', '/estado');
      this.enLinea.set(estado.disponible);
      this.cuentasDemo.set(await this.api.solicitar<AccesoDemo[]>('GET', '/demo/accesos'));
      if (this.sesion()) await this.refrescar();
    } catch {
      this.enLinea.set(false);
      this.mensaje.set('La API no está disponible. Ejecuta iniciar.bat desde Windows.');
      this.esError.set(true);
    }
    this.flujoActivo.set(this.flujos['login']);
  }

  /** Comprueba otra vez la disponibilidad de la API sin recargar el navegador. */
  async reintentarConexion(): Promise<void> {
    await this.inicializar();
    if (this.enLinea()) { this.mensaje.set(''); }
  }

  /** Copia credenciales ficticias para facilitar las demostraciones por rol. */
  usarCuenta(cuenta: AccesoDemo): void {
    this.login = { identificador: cuenta.identificador, contrasena: cuenta.contrasena };
    this.mensaje.set('');
  }

  /** Inicia sesión mediante el Controller, MediatR, Handler y servicio del backend. */
  async iniciarSesion(): Promise<void> {
    this.ocupado.set(true);
    this.mensaje.set('');
    try {
      const sesion = await this.api.solicitar<Sesion>('POST', '/autenticacion/iniciar-sesion', this.login);
      this.api.guardarSesion(sesion);
      this.vista.set('inicio');
      this.aviso('Acceso concedido. El servidor comprobó la cuenta y generó el JWT.');
      await this.refrescar();
      this.flujoActivo.set(this.flujos['login']);
    } catch (error) { this.error(error); }
    finally { this.ocupado.set(false); }
  }

  /** Limpia el acceso sin tocar los datos del servidor. */
  cerrarSesion(): void {
    this.api.cerrarSesion();
    this.vista.set('inicio');
    this.datos.set(null);
    this.miExpediente.set(null);
    this.perfil.set(null);
    this.auditorias.set([]);
    this.mensaje.set('Sesión cerrada. El servidor sigue conservando los datos de la demo.');
    this.esError.set(false);
  }

  /** Cambia la vista manteniendo el diseño compacto del panel. */
  async cambiarVista(vista: Vista): Promise<void> {
    this.vista.set(vista);
    this.mensaje.set('');
    if (vista === 'auditoria') await this.cargarAuditoria();
    if (vista === 'personal') this.flujoActivo.set(this.flujos['personal']);
    if (vista === 'perfil') await this.cargarPerfil();
    if (vista === 'expediente') await this.cargarExpediente();
  }

  /** Sincroniza los catálogos de demo o el expediente autorizado de cliente. */
  async refrescar(): Promise<void> {
    try {
      if (this.esPersonal()) {
        this.datos.set(await this.api.solicitar<ResumenDemo>('GET', '/demo/catalogos'));
        if (this.esGerente()) await this.cargarPerfil();
      } else if (this.sesion()?.rol === 'CLIENTE') {
        await this.cargarExpediente();
      }
      this.enLinea.set(true);
    } catch (error) { this.error(error); }
  }

  /** Ejecuta una acción y actualiza la pantalla después de la respuesta de la API. */
  private async ejecutar(claveFlujo: string, tarea: () => Promise<string>): Promise<void> {
    if (this.ocupado()) return;
    this.ocupado.set(true);
    this.mensaje.set('');
    this.flujoActivo.set(this.flujos[claveFlujo]);
    try {
      const resultado = await tarea();
      this.aviso(resultado);
      await this.refrescar();
      if (this.vista() === 'auditoria') await this.cargarAuditoria();
    } catch (error) { this.error(error); }
    finally { this.ocupado.set(false); }
  }

  /** Da de alta una cuenta presencial y solicita el ID generado. */
  async crearCliente(): Promise<void> {
    await this.ejecutar('clienteCrear', async () => {
      const resultado = await this.api.solicitar<{ id: number }>('POST', '/clientes', this.nuevoCliente);
      this.nuevoCliente = { identificador: '', contrasena: '', nombres: '', apellidos: '', telefono: '' };
      this.nuevaMoto.clienteId = resultado.id;
      return `Cliente #${resultado.id} registrado correctamente. Puedes vincularle una motocicleta.`;
    });
  }

  /** Desactiva una cuenta sin borrar los registros históricos. */
  async desactivarCliente(cliente: Cliente): Promise<void> {
    if (!confirm(`¿Desactivar la cuenta de ${cliente.nombres} ${cliente.apellidos}? Se conservará su expediente.`)) return;
    await this.ejecutar('clienteBaja', async () => {
      await this.api.solicitar<void>('DELETE', `/clientes/${cliente.id}`);
      return `Cuenta del cliente #${cliente.id} desactivada. El expediente permanece.`;
    });
  }

  /** Crea una cuenta de empleado o mecánico con autorización de gerencia. */
  async crearEmpleado(): Promise<void> {
    await this.ejecutar('empleado', async () => {
      const resultado = await this.api.solicitar<{ id: number }>('POST', '/personal', this.nuevoEmpleado);
      this.nuevoEmpleado = { identificador: '', contrasena: '', nombres: '', apellidos: '', esMecanico: false };
      return `Personal #${resultado.id} registrado correctamente.`;
    });
  }

  /** Registra una unidad después de comprobar VIN y cliente en el backend. */
  async crearMotocicleta(): Promise<void> {
    await this.ejecutar('moto', async () => {
      const resultado = await this.api.solicitar<{ id: number }>('POST', '/motocicletas', this.nuevaMoto);
      this.nuevaAtencion.clienteId = this.nuevaMoto.clienteId;
      this.nuevaAtencion.motocicletaId = resultado.id;
      this.nuevaMoto = { clienteId: this.nuevaMoto.clienteId, numeroSerie: '', marca: '', modelo: '',
        anio: 2024, placa: '', color: '', kilometrajeActual: 0 };
      return `Motocicleta #${resultado.id} vinculada al cliente.`;
    });
  }

  /** Ajusta el selector de motocicleta al propietario recién elegido. */
  seleccionarClienteAtencion(): void {
    const primera = this.motosDelCliente(this.nuevaAtencion.clienteId)[0];
    this.nuevaAtencion.motocicletaId = primera?.id ?? 0;
  }

  /** Abre una atención con un mecánico habilitado. */
  async recibirMoto(): Promise<void> {
    await this.ejecutar('recepcion', async () => {
      const resultado = await this.api.solicitar<{ id: number }>('POST', '/atenciones', this.nuevaAtencion);
      this.diagnostico.atencionId = resultado.id;
      this.cobro.atencionId = resultado.id;
      this.consultarId = resultado.id;
      this.nuevaAtencion.motivo = '';
      return `Atención #${resultado.id} creada en estado RECIBIDA.`;
    });
  }

  /** Registra hallazgos únicamente para una atención recibida. */
  async registrarDiagnostico(): Promise<void> {
    await this.ejecutar('diagnostico', async () => {
      await this.api.solicitar<void>('POST', `/atenciones/${this.diagnostico.atencionId}/diagnostico`,
        { hallazgos: this.diagnostico.hallazgos });
      this.cobro.atencionId = this.diagnostico.atencionId;
      this.diagnostico.hallazgos = '';
      return `Diagnóstico registrado en la atención #${this.diagnostico.atencionId}.`;
    });
  }

  /** Registra el cobro fijo solo después de diagnosticar y una sola vez. */
  async cobrarRevision(): Promise<void> {
    await this.ejecutar('cobro', async () => {
      const resultado = await this.api.solicitar<{ id: number }>('POST',
        `/atenciones/${this.cobro.atencionId}/cobro`, { referenciaComprobante: this.cobro.referenciaComprobante });
      this.cobro.referenciaComprobante = '';
      return `Cobro #${resultado.id} registrado a la tarifa fija de demostración.`;
    });
  }

  /** Consulta una atención aplicando autorización por rol y propiedad. */
  async consultarAtencion(id = this.consultarId): Promise<void> {
    await this.ejecutar('atencion', async () => {
      const atencion = await this.api.solicitar<Atencion>('GET', `/atenciones/${id}`);
      this.atencionConsultada.set(atencion);
      return `Consulta de atención #${id} autorizada.`;
    });
  }

  /** Lee la lista de cambios, exclusivamente con el rol de gerente. */
  async cargarAuditoria(): Promise<void> {
    if (!this.esGerente()) return;
    this.flujoActivo.set(this.flujos['auditoria']);
    try { this.auditorias.set(await this.api.solicitar<Auditoria[]>('GET', '/auditoria?limite=50')); }
    catch (error) { this.error(error); }
  }

  /** Consulta el perfil del gerente desde su JWT, sin pasar un ID arbitrario. */
  async cargarPerfil(): Promise<void> {
    if (!this.esGerente()) return;
    this.flujoActivo.set(this.flujos['gerente']);
    try { this.perfil.set(await this.api.solicitar<PerfilGerente>('GET', '/gerencia/mi-perfil')); }
    catch (error) { this.error(error); }
  }

  /** Recupera el expediente y las motocicletas del cliente autenticado. */
  async cargarExpediente(): Promise<void> {
    if (this.sesion()?.rol !== 'CLIENTE') return;
    this.flujoActivo.set(this.flujos['expediente']);
    try { this.miExpediente.set(await this.api.solicitar<Expediente>('GET', '/clientes/mi-expediente')); }
    catch (error) { this.error(error); }
  }

  /** Muestra el recorrido de la consulta privada de motocicletas. */
  async consultarMisMotocicletas(): Promise<void> {
    await this.ejecutar('motosPropias', async () => {
      const motos = await this.api.solicitar<Motocicleta[]>('GET', '/motocicletas/mis-motocicletas');
      return `La API devolvió ${motos.length} motocicleta(s) vinculada(s) a tu identidad.`;
    });
  }

  /** Filtra las motocicletas del cliente seleccionado en el formulario. */
  motosDelCliente(clienteId: number): Motocicleta[] {
    return this.datos()?.motocicletas.filter(x => x.clienteId === Number(clienteId)) ?? [];
  }

  /** Presenta el apellido y nombre de un propietario registrado. */
  nombreCliente(id: number): string {
    const cliente = this.datos()?.clientes.find(x => x.id === id);
    return cliente ? `${cliente.nombres} ${cliente.apellidos}` : `Cliente #${id}`;
  }

  /** Presenta el nombre de un mecánico registrado. */
  nombreMecanico(id: number): string {
    const empleado = this.datos()?.personal.find(x => x.id === id);
    return empleado ? `${empleado.nombres} ${empleado.apellidos}` : `Mecánico #${id}`;
  }

  /** Activa el formulario de diagnóstico para la fila seleccionada. */
  prepararDiagnostico(atencion: Atencion): void {
    this.diagnostico.atencionId = atencion.id;
    this.cobro.atencionId = atencion.id;
    this.consultarId = atencion.id;
    this.vista.set('atenciones');
    this.flujoActivo.set(this.flujos['diagnostico']);
    document.getElementById('operaciones-atencion')?.scrollIntoView({ behavior: 'smooth' });
  }

  /** Expone una notificación de confirmación breve para la demostración. */
  private aviso(texto: string): void { this.mensaje.set(texto); this.esError.set(false); }

  /** Expone un error real de la API sin simular operaciones exitosas. */
  private error(error: unknown): void { this.mensaje.set(mensajeError(error)); this.esError.set(true); }

  /** Formatea una fecha sin asumir una zona horaria fija del servidor. */
  fecha(valor: string | null | undefined): string {
    return valor ? new Date(valor).toLocaleString('es-MX', { dateStyle: 'short', timeStyle: 'short' }) : '—';
  }
}
