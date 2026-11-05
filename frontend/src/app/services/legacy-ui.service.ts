import { Injectable } from '@angular/core';

@Injectable({ providedIn: 'root' })
export class LegacyUiService {
  // Adaptador transitorio: ejecuta las vistas originales sin rehacer el trabajo de Fernando.
  // Los archivos de ./js, ./css y ./assets siguen siendo los mismos del PR #14.
  load(): Promise<void> {
    return new Promise((resolve, reject) => {
      const script = document.createElement('script');
      script.type = 'module';
      script.src = 'js/app.js';
      script.dataset['followMotorsLegacy'] = 'true';
      script.onload = () => resolve();
      script.onerror = () => reject(new Error('No se pudieron cargar las pantallas del frontend.'));
      document.body.appendChild(script);
    });
  }
}
