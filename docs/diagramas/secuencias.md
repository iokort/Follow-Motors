# Diagramas de secuencia — casos críticos

Los diagramas describen el comportamiento esperado. No suponen que el flujo de negocio ya esté implementado.

## US01 / US02 — Login autenticado y autorizado en servidor

```mermaid
sequenceDiagram
  autonumber
  actor Persona as Gerente/Cliente
  participant View as Vista HTML
  participant VM as LoginViewModel
  participant API as API C#
  participant Auth as AutenticacionServicio
  participant DB as MySQL
  Persona->>View: Introducir credenciales
  View->>VM: Enviar formulario
  VM->>API: POST /api/v1/auth/login (propuesto)
  API->>Auth: Validar credenciales
  Auth->>DB: Consultar hash y estado de usuario
  DB-->>Auth: Identidad y hash
  alt Credenciales y rol autorizados
    Auth-->>API: Sesión validada
    API-->>VM: Cookie segura / respuesta permitida
    VM-->>View: Abrir área privada autorizada
  else Credenciales inválidas
    Auth-->>API: Error neutro
    API-->>VM: 401 no autorizado
    VM-->>View: Error sin revelar existencia de cuenta
  end
```

## US07 — Registro de mantenimiento

```mermaid
sequenceDiagram
  autonumber
  actor Operador as Gerente/Mecánico
  participant UI as TallerViewModel
  participant API as ControladorMantenimiento
  participant Auth as AutorizadorRecurso
  participant App as GestorMantenimiento
  participant Repo as Repositorios
  participant DB as MySQL
  Operador->>UI: Registrar servicio y kilometraje
  UI->>API: Solicitud autenticada (ruta propuesta)
  API->>Auth: ¿Permiso para la motocicleta/orden?
  alt Permiso denegado
    Auth-->>API: No autorizado
    API-->>UI: 403
  else Permiso válido
    API->>App: Registrar cambio
    App->>Repo: Guardar orden e historial en transacción
    Repo->>DB: Escrituras atómicas
    DB-->>Repo: Confirmación
    Repo-->>App: Resultado
    App-->>API: Servicio registrado
    API-->>UI: Estado e historial actualizados
  end
```

Estos diagramas necesitan pruebas con datos ficticios y validación de permisos cuando exista el código.

## US04 — Registro presencial por asesor

```mermaid
sequenceDiagram
  autonumber
  actor Asesor as Asesor de sucursal
  participant UI as Interfaz privada
  participant API as API C#
  participant RBAC as Autorizacion
  participant Alta as GestorClientes
  participant DB as MySQL
  Asesor->>UI: Verificar cliente y motocicleta
  UI->>API: Solicitud de alta autenticada
  API->>RBAC: Comprobar permiso
  alt No autorizado
    RBAC-->>API: Denegado
    API-->>UI: 403
  else Autorizado
    API->>Alta: Alta y vinculacion validada
    Alta->>DB: Transaccion y auditoria
    DB-->>Alta: Confirmacion
    Alta-->>API: Registro confirmado
    API-->>UI: Expediente habilitado
  end
```

La verificación de identidad y entrega de credenciales son procedimientos pendientes de aprobar con sucursal.
