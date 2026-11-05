import { readFileSync, existsSync } from 'node:fs';
import { test } from 'node:test';
import assert from 'node:assert/strict';

const root = new URL('../', import.meta.url);
const read = path => readFileSync(new URL(path, root), 'utf8');

test('Angular usa las hojas de estilo originales de Fernando', () => {
  const workspace = JSON.parse(read('angular.json'));
  const opts = workspace.projects['follow-motors'].architect.build.options;
  assert.equal(workspace.projects['follow-motors'].architect.build.builder, '@angular/build:application');
  assert.ok(opts.styles.includes('css/components/dashboard.css'));
  assert.ok(opts.styles.includes('css/components/citas.css'));
  assert.ok(opts.assets.some(a => a.input === 'js' && a.output === 'js'));
  assert.ok(opts.assets.some(a => a.input === 'assets' && a.output === 'assets'));
});

test('El frontend de Fernando y sus vistas permanecen intactos', () => {
  for (const name of [
    'js/views/ClienteView.js',
    'js/views/GerenteView.js',
    'js/views/gerente/GestionCitasView.js',
    'js/views/gerente/GestionGarantiasView.js',
    'assets/images/branding/logo.png'
  ]) {
    assert.ok(existsSync(new URL(name, root)), name);
  }
});

test('MVVM separa View, ViewModel y adaptador', () => {
  assert.match(read('src/app/app.ts'), /inject\(AppViewModel\)/);
  assert.match(read('src/app/viewmodels/app.viewmodel.ts'), /signal\(/);
  assert.match(read('src/app/services/legacy-ui.service.ts'), /js\/app\.js/);
  assert.match(read('src/app/app.html'), /id="navbar"/);
  assert.match(read('src/app/app.html'), /id="app"/);
  assert.match(read('src/app/app.html'), /id="retry-api"/);
});

test('La aplicación cliente permanece HTTP, nunca conecta a MySQL', () => {
  const api = read('js/api.js');
  assert.match(api, /\/api\/v1/);
  assert.doesNotMatch(api, /mysql:|SELECT\s|INSERT\s/i);
});
