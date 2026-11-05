# Actores y funcionalidades — Mapa de casos de uso

Los actores operativos no equivalen a roles del equipo SSC. Este diagrama Mermaid es **un mapa funcional**, no sustituye los casos de uso textuales ni un diagrama UML formal.

```mermaid
flowchart LR
  Visitante["Visitante"]
  Cliente["Cliente"]
  Gerente["Gerente"]
  Asesor["Asesor"]
  Mecanico["Mecánico"]
  subgraph Publica["Web pública"]
    Inicio["US09 · Portada"]
    Catalogo["US10 · Catálogo"]
    FAQ["US11 · Información"]
    Contacto["US12 · Contacto"]
  end
  subgraph Privada["Área privada"]
    Login["US01/02 · Iniciar sesión"]
    Logout["US03 · Salir"]
    MisMotos["US08 · Mis motocicletas e historial"]
    Km["US13 · Actualizar km"]
    Solicitar["US14 · Solicitar servicio"]
  end
  subgraph Taller["Operación del taller"]
    Alta["US04 · Alta cliente y moto"]
    Editar["US05 · Administración"]
    Agenda["US06 · Agenda"]
    Servicio["US07 · Mantenimiento"]
    Asesoria["US15 · Agenda asesor"]
    Ordenes["US16 · Órdenes mecánico"]
    Resumen["US17 · Resumen gerencial"]
    Auth["US21 · Roles y permisos"]
  end
  Visitante --> Inicio
  Visitante --> Catalogo
  Visitante --> FAQ
  Visitante --> Contacto
  Cliente --> Login
  Cliente --> Logout
  Cliente --> MisMotos
  Cliente --> Km
  Cliente --> Solicitar
  Gerente --> Login
  Gerente --> Alta
  Gerente --> Editar
  Gerente --> Agenda
  Gerente --> Servicio
  Gerente --> Resumen
  Asesor --> Asesoria
  Mecanico --> Ordenes
  Auth -.-> Login
  Auth -.-> Alta
  Auth -.-> MisMotos
  Auth -.-> Ordenes
```

Los módulos futuros US18, US19, US20 y US22 no se consideran parte del alcance mínimo hasta la aprobación del negocio.
