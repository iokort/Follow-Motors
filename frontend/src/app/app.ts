import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { HealthService } from './services/health.service';

type ApiStatus = 'loading' | 'available' | 'unavailable';

@Component({
  selector: 'app-root',
  templateUrl: './app.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class AppComponent {
  private readonly health = inject(HealthService);
  readonly apiStatus = signal<ApiStatus>('loading');

  constructor() {
    void this.checkApi();
  }

  async checkApi(): Promise<void> {
    this.apiStatus.set('loading');
    try {
      const result = await firstValueFrom(this.health.check());
      this.apiStatus.set(result.status === 'ok' ? 'available' : 'unavailable');
    } catch {
      this.apiStatus.set('unavailable');
    }
  }
}
