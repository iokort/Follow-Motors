import { readFileSync } from 'node:fs';
import { test } from 'node:test';
import assert from 'node:assert/strict';

const read = (name) => readFileSync(new URL('../' + name, import.meta.url), 'utf8');

test('Angular es el builder oficial del frontend', () => {
  const workspace = JSON.parse(read('angular.json'));
  assert.equal(workspace.projects['follow-motors'].architect.build.builder, '@angular/build:application');
});

test('el servicio consume API sin SQL directo', () => {
  const source = read('src/app/services/health.service.ts');
  assert.match(source, /\/api\/v1\/health/);
  assert.doesNotMatch(source, /mysql|SELECT\s|INSERT\s/i);
});

test('el footer pertenece a ioKort', () => {
  const html = read('src/app/app.html');
  assert.match(html, /© 2026 Kurt Cobain Vázquez Sánchez/);
  assert.match(html, /https:\/\/iokort\.com/);
});

test('MVVM: el componente delega estado y acciones al ViewModel', () => {
  const component = read('src/app/app.ts');
  const viewmodel = read('src/app/viewmodels/health.viewmodel.ts');
  const view = read('src/app/app.html');
  assert.match(component, /inject\(HealthViewModel\)/);
  assert.match(viewmodel, /signal<ApiStatus>/);
  assert.match(viewmodel, /HealthService/);
  assert.match(view, /vm\.apiStatus\(\)/);
  assert.match(view, /vm\.checkApi\(\)/);
});
