import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { HealthViewModel } from './viewmodels/health.viewmodel';

@Component({
  selector: 'app-root',
  templateUrl: './app.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class AppComponent {
  readonly vm = inject(HealthViewModel);

  constructor() {
    void this.vm.checkApi();
  }
}
