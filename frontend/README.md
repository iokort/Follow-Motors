# Angular 22 y MVVM sobre la interfaz original de Fernando

El frontend **activo en desarrollo** se ejecuta desde Angular 22 con TypeScript. Conserva las vistas, servicios, estilos y recursos creados por Vixtor Fernando; **no se reescribieron ni reemplazaron**.

## Ejecutar con Git CMD

Dentro de `frontend`:

```cmd
npm install
npm start
```

Abrir http://localhost:4200 (el proxy de `/api` apunta a la API local en http://localhost:5099).

```cmd
npm run build
npm test
```

## Integración incremental

- Angular gestiona el arranque y un ViewModel de inicialización.
- El adaptador `src/app/services/legacy-ui.service.ts` monta **temporalmente** las pantallas originales (`js/app.js`) en el DOM de Angular.
- Los módulos JS/HTML de Fernando aún usan su enrutador por hash y datos de demostración: **no son componentes Angular nativos todavía**.
- Portar internamente cada pantalla al patrón MVVM, **conservando su composición, estilos y comportamiento**, será trabajo incremental coordinado con Fernando.
- El componente Angular no pretende que ya estén implementadas la autenticación, la agenda real o las garantías con MySQL.
- El backend sigue en C# / ASP.NET Core .NET 10 y los datos en MySQL V3 de Gael.

## Referencia histórica del frontend original

# Frontend — Víctor

Punto de partida: `index.html`, `css/styles.css`, `js/app.js` y `js/api.js`.

## Ejecutar
Con cualquier servidor HTTP estático en puerto **5500** (por ejemplo, extensión Live Server de VS Code). No abrir como `file://`.

La página consulta `GET /api/v1/health` en `http://localhost:5099`. El URL se puede sobreescribir mediante `window.FOLLOW_MOTORS_API_URL` antes de cargar `app.js` si cambia el entorno. Para hosts separados, coordinar CORS con Kort.

La página es **un scaffold técnico, no el diseño final**. No incluir credenciales, llamadas directas a MySQL ni inventar respuestas definitivas. Trabajar en `feature/frontend` y acordar el contrato de API antes de implementar cada pantalla.

