# Frontend — Vixtor Fernando

Punto de partida: `index.html`, `css/styles.css`, `js/app.js` y `js/api.js`.

## Ejecutar
Con cualquier servidor HTTP estático en puerto **5500** (por ejemplo, extensión Live Server de VS Code). No abrir como `file://`.

La página consulta `GET /api/v1/health` en `http://localhost:5099`. El URL se puede sobreescribir mediante `window.FOLLOW_MOTORS_API_URL` antes de cargar `app.js` si cambia el entorno. Para hosts separados, coordinar CORS con Kort.

La página es **un scaffold técnico, no el diseño final**. No incluir credenciales, llamadas directas a MySQL ni inventar respuestas definitivas. Trabajar en `feature/frontend` y acordar el contrato de API antes de implementar cada pantalla.
