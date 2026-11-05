import { inject, Injectable, signal } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { HealthService } from '../services/health.service';

export type ApiStatus = 'loading' | 'available' | 'unavailable';

@Injectable({ providedIn: 'root' })
export class HealthViewModel {
  private readonly health = inject(HealthService);
  readonly apiStatus = signal<ApiStatus>('loading');

  async checkApi(): Promise<void> {
    this.apiStatus.set('loading');
    try {
      const response = await firstValueFrom(this.health.check());
      this.apiStatus.set(response.status === 'ok' ? 'available' : 'unavailable');
    } catch {
      this.apiStatus.set('unavailable');
    }
  }
}
