import { HttpClient, HttpErrorResponse, HttpHeaders } from '@angular/common/http';
import { Injectable, inject, signal } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { Sesion } from './models';

/** Adaptador HTTP: Angular nunca accede a MySQL ni a la memoria ficticia directamente. */
@Injectable({ providedIn: 'root' })
export class ApiService {
  private readonly http = inject(HttpClient);
  private readonly claveSesion = 'followmotors-demo-sesion';
  readonly sesion = signal<Sesion | null>(this.leerSesion());

  /** Envía las peticiones a ASP.NET Core a través del proxy de Angular. */
  async solicitar<T>(metodo: 'GET' | 'POST' | 'DELETE', ruta: string, cuerpo?: unknown): Promise<T> {
    const sesion = this.sesion();
    let headers = new HttpHeaders();
    if (sesion?.token) headers = headers.set('Authorization', `Bearer ${sesion.token}`);
    try {
      return await firstValueFrom(this.http.request<T>(metodo, `/api${ruta}`, { body: cuerpo, headers }));
    } catch (error) {
      if (error instanceof HttpErrorResponse && error.status === 401 && ruta !== '/autenticacion/iniciar-sesion') {
        this.cerrarSesion();
      }
      throw error;
    }
  }

  /** Guarda el JWT solo durante la sesión de la pestaña del navegador. */
  guardarSesion(sesion: Sesion): void {
    this.sesion.set(sesion);
    sessionStorage.setItem(this.claveSesion, JSON.stringify(sesion));
  }

  /** Elimina el JWT y bloquea las vistas privadas. */
  cerrarSesion(): void {
    this.sesion.set(null);
    sessionStorage.removeItem(this.claveSesion);
  }

  /** Recupera la sesión de demostración, descartando tokens caducados. */
  private leerSesion(): Sesion | null {
    try {
      const guardado = sessionStorage.getItem(this.claveSesion);
      if (!guardado) return null;
      const sesion = JSON.parse(guardado) as Sesion;
      if (!sesion.token || new Date(sesion.expiraEnUtc).getTime() <= Date.now()) {
        sessionStorage.removeItem(this.claveSesion);
        return null;
      }
      return sesion;
    } catch {
      return null;
    }
  }
}

/** Convierte errores de validación, autorización y red en mensajes claros. */
export function mensajeError(error: unknown): string {
  if (error instanceof HttpErrorResponse) {
    if (error.status === 0) return 'No se pudo conectar con la API. Comprueba que iniciar.bat abrió el servidor C#.';
    const problema = error.error as { detail?: string; title?: string; errores?: { campo: string; mensaje: string }[] } | null;
    if (problema?.errores?.length) return problema.errores.map(x => x.mensaje).join(' · ');
    if (problema?.detail) return problema.detail;
    if (error.status === 401) return 'Credenciales incorrectas, sesión caducada o cuenta desactivada.';
    if (error.status === 403) return 'Este rol no tiene permiso para ejecutar la operación.';
    if (error.status === 404) return 'No se encontró el registro solicitado.';
    return `Error HTTP ${error.status}. ${problema?.title || 'No se pudo completar la operación.'}`;
  }
  return error instanceof Error ? error.message : 'Error inesperado.';
}
