# Flujo de colaboración

## Ramas y alcance

- `main` conserva la referencia principal. No desarrollar directamente en ella.
- `develop` es la rama de integración prevista; primero revisar su divergencia con `main` en [SCRUM-2](https://iokort.atlassian.net/browse/SCRUM-2). No fusionar ni sobrescribir ramas históricas sin revisar los cambios.
- Cada requisito tiene su rama `feature/SCRUM-<clave>-<tema>`. Hasta resolver la integración, partir de la referencia comprobada de `main` y no mezclar automáticamente trabajo histórico. Una subtarea puede tener su propia rama con su clave si requiere revisión separada.

## Ciclo de trabajo

1. Consultar Jira: alcance, criterios, responsable y dependencias; mover a **En curso** cuando se inicie trabajo real.
2. Nombrar ramas, commits y PR con la clave Jira; por ejemplo, `SCRUM-5: validar sesión`.
3. En el PR incluir enlace a Jira, resumen, pruebas y efectos en permisos, datos o interfaz. Solicitar revisión a quien corresponda al área.
4. Mover a **En revisión** cuando el PR y las evidencias estén disponibles. Ante observaciones, pasar a **Ajustando**.
5. Pasar a **Finalizado** únicamente tras aceptación, pruebas y revisión, con el cambio integrado cuando corresponda.

## Criterios de entrega

- Criterios de aceptación satisfechos y verificables; casos positivos, negativos y permisos según aplique.
- Cambios MySQL revisados por el responsable de datos; contratos API coordinados con frontend.
- Pruebas y fallos documentados en Jira; sin credenciales, datos personales de clientes ni archivos de configuración con secretos.
- Documentación actualizada. No usar el README ni ramas históricas como prueba de despliegue o funcionalidad.
