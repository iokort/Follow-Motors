import { getHealth } from './api.js';

const status = document.querySelector('#api-status');
const retry = document.querySelector('#retry-api');

async function checkApi() {
  status.textContent = 'Comprobando conexión…';
  retry.disabled = true;
  try {
    const data = await getHealth();
    status.textContent = data.status === 'ok'
      ? 'API disponible (el resto de los módulos sigue en desarrollo).'
      : 'La API respondió con un estado inesperado.';
  } catch {
    status.textContent = 'API sin conexión. Ejecuta el backend e inténtalo nuevamente.';
  } finally {
    retry.disabled = false;
  }
}

retry.addEventListener('click', checkApi);
checkApi();
