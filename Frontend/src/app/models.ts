/** Sesión que entrega el backend C#; se guarda únicamente durante la pestaña actual. */
export interface Sesion {
  usuarioId: number;
  identificador: string;
  rol: 'GERENTE' | 'EMPLEADO' | 'CLIENTE';
  token: string;
  expiraEnUtc: string;
}

/** Credenciales inventadas obtenidas de DemoDatosEnMemoria.cs. */
export interface AccesoDemo {
  identificador: string;
  contrasena: string;
  rol: string;
  nombre: string;
}

export interface Cliente {
  id: number;
  usuarioId: number;
  nombres: string;
  apellidos: string;
  telefono?: string | null;
  identificador?: string;
  activo?: boolean;
}

export interface Personal {
  id: number;
  usuarioId: number;
  nombres: string;
  apellidos: string;
  esMecanico: boolean;
  disponible: boolean;
  rol: string;
}

export interface Motocicleta {
  id: number;
  clienteId: number;
  numeroSerie: string;
  placa?: string | null;
  marca: string;
  modelo: string;
  anio?: number | null;
  color?: string | null;
  kilometrajeActual: number;
}

export interface Atencion {
  id: number;
  clienteId: number;
  motocicletaId: number;
  citaId?: number | null;
  mecanicoId: number;
  motivo: string;
  kilometrajeIngreso: number;
  textoDiagnostico?: string | null;
  estado: 'RECIBIDA' | 'DIAGNOSTICADA' | string;
  ingresadaEn: string;
  diagnosticadaEn?: string | null;
}

export interface Cobro {
  id: number;
  atencionId: number;
  importe: number;
  fecha: string;
  referencia?: string | null;
}

export interface Auditoria {
  id: number;
  usuarioId?: number | null;
  entidad: string;
  registroId: number;
  accion: string;
  antes?: string | null;
  despues?: string | null;
  registradaEn: string;
}

export interface ResumenDemo {
  clientes: Cliente[];
  motocicletas: Motocicleta[];
  personal: Personal[];
  atenciones: Atencion[];
  cobros: Cobro[];
  auditorias: number;
  tarifaRevision: number;
}

export interface Expediente {
  cliente: Cliente;
  motocicletas: Motocicleta[];
}

export interface PerfilGerente {
  id: number;
  nombres: string;
  apellidos: string;
  disponible: boolean;
}

export interface EstadoApi {
  disponible: boolean;
  modo: string;
  servicio: string;
}

/** Archivos a mostrar durante la explicación del caso de uso en Visual Studio. */
export interface Flujo {
  titulo: string;
  objetivo: string;
  ruta: string;
  archivos: string[];
}
