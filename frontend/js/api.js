// Punto único de acceso HTTP. No contiene secretos ni lógica SQL.
const DEFAULT_API_URL = 'http://localhost:5099/api/v1';

function getBaseUrl() {
  return (window.FOLLOW_MOTORS_API_URL || DEFAULT_API_URL).replace(/\/+$/, '');
}

export async function getHealth({ signal } = {}) {
  const response = await fetch(`${getBaseUrl()}/health`, {
    method: 'GET',
    headers: { Accept: 'application/json' },
    signal
  });

  if (!response.ok) {
    throw new Error(`HTTP ${response.status}`);
  }

  return response.json();
}
