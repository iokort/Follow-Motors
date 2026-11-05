import { AfterViewInit, ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { AppViewModel } from './viewmodels/app.viewmodel';

@Component({
  selector: 'app-root',
  templateUrl: './app.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class AppComponent implements AfterViewInit {
  readonly vm = inject(AppViewModel);

  ngAfterViewInit(): void {
    void this.vm.initialize();
  }
}
