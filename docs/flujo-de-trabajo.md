# Flujo Git y reglas de colaboración

## Estrategia de ramas

- `main`: versión estable y candidata a despliegue. Prohibido usarla como rama de trabajo.
- `develop`: integración colaborativa; todos los PR ordinarios apuntan aquí.
- `feature/backend`, `feature/database`, `feature/frontend`, `feature/docs`: ramas base de área existentes. No son exclusivas de una persona ni necesariamente para dejar trabajo inacabado durante meses.
- Preferir una rama **por Issue** nacida de `develop`: `feature/12-login`, `fix/18-kilometraje`, `docs/21-manual-cliente`. Esto evita que diferentes cambios se bloqueen entre sí.

## Ciclo de una tarea

1. Aarón o el responsable documenta Issue con objetivo, alcance, criterios de aceptación y dependencias.
2. Identificar quién aprueba esquema (Gael), contrato API (Kort) y datos de la interfaz (Fernando), si aplica.
3. Crear rama de trabajo desde el último `develop`.
4. Desarrollar sin incorporar secretos; probar en local. No cambiar production ni datos reales.
5. Abrir PR hacia `develop`, con Issue, evidencia de pruebas y cambios incompatibles señalados.
6. CI en verde, un revisor **diferente al autor** y conversaciones resueltas.
7. Integrar con `squash`; actualizar documentación y cerrar Issue al verificarse.
8. Crear PR `develop` → `main` para versión aprobada y hacer despliegue manual con plan de reversión.

## Coordinación

- El frontend consume la API; no accede directamente a MySQL.
- El backend valida datos, roles y permisos; no confía en validaciones del navegador.
- Gael versiona cambios de esquema; **no ejecutar migraciones destructivas sin respaldo y aprobación explícita**.
- Cambiar nombres de campos o rutas requiere actualizar contratos y avisar a las otras áreas.
- Commit convencional: `feat(api): ...`, `feat(frontend): ...`, `feat(db): ...`, `fix: ...`, `docs: ...`.
- El mismo PR no debe mezclar refactors masivos con nuevas funciones si se pueden separar.
- PR sin prueba o sin documento de excepción no es entregable.

## Comandos mínimos (Git CMD)

```cmd
git fetch origin
git switch develop
git pull --ff-only origin develop
git switch -c feature/numero-descripcion
rem Desarrollar y verificar
git add .
git commit -m "feat(frontend): describir cambio"
git push -u origin feature/numero-descripcion
gh pr create --base develop --fill
```

Los cambios a `main` requieren PR, verificación y aprobación; no hay despliegue automático configurado.
