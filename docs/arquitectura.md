# Arquitectura de Follow Motors

```text
    Navegador
(HTML + CSS + JavaScript)
        |
        | HTTP(S) / JSON
        v
 ASP.NET Core Web API
      C# / .NET 10
        |
        | MySqlConnector / consultas parametrizadas
        v
      MySQL 8.4
```

## Estado implementado
- `backend/FollowMotors.Api`: servicio web mínimo con endpoint `GET /api/v1/health`.
- `Infrastructure/IDatabaseConnectionFactory.cs`: contrato de fábrica de conexiones, sin imponer repositorios todavía.
- `GET /api/v1/health/database`: comprueba conectividad MySQL solo en Development; no muestra secretos.
- `frontend/`: pantalla base y cliente HTTP en `js/api.js`; no representa el diseño final.
- `compose.yaml`: MySQL local persistente y contenedor de API para desarrollo.
- `.github/workflows/ci.yml`: validaciones automáticas, aún sujetas a resultados de ejecución remota.

## Límites de responsabilidad
- **Frontend:** interfaz, estado visual, interacción y consumo de API. No tiene acceso SQL.
- **Backend:** reglas de negocio, validaciones del servidor, permisos y respuesta HTTP.
- **MySQL:** persistencia, integridad, migraciones versionadas y restricciones.
- **Documentación:** requisitos, casos de uso, contratos y manuales.

SOLID: separación clara de responsabilidades y dependencias cuando sean necesarias para comprobar el código. No construir controladores, servicios o entidades que aún no tienen requisitos.

## Riesgos y decisiones pendientes
- Los nombres de módulos del sistema anterior no certifican su funcionamiento.
- Primero acordar requisitos, esquema de Gael y formatos JSON con Vixtor Fernando.
- Definir autenticación/autorización antes de cualquier operación real sobre usuarios.
- Verificar alojamiento compatible con .NET 10. El sitio actual funciona con infraestructura web existente, no constituye evidencia de que pueda hospedar un servicio ASP.NET Core.
- Producción no debe alterarse durante la etapa de reconstrucción.
