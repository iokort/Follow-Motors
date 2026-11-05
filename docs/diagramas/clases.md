# Diagrama de clases — núcleo de dominio y aplicación

Vista conceptual de relaciones tomadas del catálogo CRC inicial. Los repositorios, controladores y ViewModels adicionales se detallan en [CRC extendidas](../crc/README.md). Las relaciones indican colaboración conceptual, no propiedades C# ya compiladas.

```mermaid
classDiagram
  direction LR
  class Usuario {
    +Identificador
    +Correo
    +Estado
  }
  class Cliente {
    +NumeroCliente
    +DatosContacto
  }
  class Sesion {
    +Vencimiento
    +Invalidar()
  }
  class Motocicleta {
    +VIN
    +Modelo
    +Kilometraje
  }
  class CitaServicio {
    +Fecha
    +Estado
  }
  class RegistroServicio {
    +Fecha
    +Kilometraje
    +Etapa
  }
  class PlanMantenimiento {
    +CalcularProximoServicio()
  }
  class SolicitudServicio {
    +Motivo
    +Estado
  }
  class AutenticacionServicio {
    +IniciarSesion()
    +CerrarSesion()
  }
  class GestorClientes {
    +Registrar()
    +Editar()
  }
  class GestorMotocicletas {
    +Registrar()
    +Consultar()
  }
  class GestorCitas {
    +Agendar()
    +Reprogramar()
  }
  class GestorMantenimiento {
    +RegistrarServicio()
    +CambiarEtapa()
  }
  class GestorSolicitudes {
    +Registrar()
    +Consultar()
  }
  Usuario "1" --> "0..1" Cliente : identidad opcional
  Usuario "1" --> "0..*" Sesion : sesiones
  Cliente "1" --> "0..*" Motocicleta : posee
  Motocicleta "1" --> "0..*" CitaServicio : agenda
  Motocicleta "1" --> "0..*" RegistroServicio : historial
  Motocicleta "1" --> "0..*" SolicitudServicio : solicitudes
  Motocicleta --> PlanMantenimiento : utiliza
  AutenticacionServicio ..> Usuario : verifica
  AutenticacionServicio ..> Sesion : gestiona
  GestorClientes ..> Cliente : administra
  GestorMotocicletas ..> Motocicleta : administra
  GestorCitas ..> CitaServicio : administra
  GestorMantenimiento ..> RegistroServicio : registra
  GestorSolicitudes ..> SolicitudServicio : gestiona
```

**Nota:** los detalles de multiplicidad y nombres concretos deberán revisarse con Gael al aprobar el esquema MySQL. Los roles gerenciales se modelan como permisos/composición, no necesariamente herencia.
