import { inject, Injectable, signal } from '@angular/core';
import { LegacyUiService } from '../services/legacy-ui.service';

@Injectable({ providedIn: 'root' })
export class AppViewModel {
  private readonly ui = inject(LegacyUiService);

  readonly loading = signal(true);
  readonly error = signal<string | null>(null);

  async initialize(): Promise<void> {
    try {
      await this.ui.load();
    } catch {
      this.error.set('La interfaz no pudo iniciarse. Recarga la página.');
    } finally {
      this.loading.set(false);
    }
  }
}
