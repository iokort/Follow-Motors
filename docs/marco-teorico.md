# Marco teórico y fundamentación — Follow Motors

## 1. Contexto del problema

El sistema actual sirve como referencia de un taller de motocicletas que necesita gestionar clientes, vehículos, citas, kilometraje, servicios e historial. En la documentación de la versión V5 se describe persistencia en el navegador y control de roles en JavaScript. Esta arquitectura no garantiza consistencia entre dispositivos ni protección de recursos privados. El rediseño plantea navegación pública de asistencia y posventa, y datos centralizados con una API que autorice cada operación. El cliente y sus motocicletas los registra presencialmente un asesor autorizado; no hay autorregistro por internet.

**Pregunta de ingeniería:** ¿Cómo separar información pública y operaciones privadas, centralizando datos e historial, con una estructura modular y una evolución incremental verificable?

## 2. Ingeniería de requisitos

Los requisitos funcionales expresan comportamientos del sistema; los no funcionales delimitan seguridad, accesibilidad, rendimiento, usabilidad y mantenibilidad. Las historias de usuario concretan actor, necesidad y valor esperado. Cada historia requiere criterios de aceptación reproducibles y trazabilidad `US → RF → interfaz → endpoint → prueba`.

En Follow Motors, la ayuda pública no requiere login, pero no incluye ventas ni creación de cuentas. El portal privado solo puede mostrar motocicletas vinculadas a su titular. Las acciones privadas requieren autenticación y autorización del servidor.

## 3. Desarrollo ágil, Scrum y Kanban

Scrum permite organizar incrementos con Product Goal, Sprint Goal, Product Backlog, Sprint Backlog y un Increment sujeto a Definition of Done (Schwaber & Sutherland, 2020). Kanban hace visible el flujo, fija políticas de inicio/fin y límites WIP y mide throughput, cycle time, WIP y work item age (Coleman & Vacanti, 2025). El equipo combinará ambos: cadencia de sprint y trabajo visualizado en GitHub Projects, sin declarar ceremonias celebradas antes de registrarlas.

## 4. Programación orientada a objetos y CRC

Las tarjetas Clase–Responsabilidad–Colaborador ayudan a descubrir responsabilidades cohesivas y las dependencias entre objetos (Beck & Cunningham, 1989). En Follow Motors se aplican a Usuario, Cliente, Motocicleta, Cita, Sesión y servicios de aplicación, antes de decidir las clases definitivas en C#.

## 5. MVVM y separación de responsabilidades

MVVM aísla la interfaz de la lógica de presentación y los modelos, facilitando pruebas y cambio de vista. En Angular, las plantillas HTML/CSS funcionan como View, los componentes y signals mantienen el estado de presentación y los servicios HttpClient consumen la API C#. El proyecto adopta MVVM en Angular, separando View, ViewModel y Model sin cambiar a .NET MAUI.

## 6. Principios SOLID, modularidad y datos

Los principios SOLID guían la cohesión, los contratos y el acoplamiento. Se propone un monolito modular ASP.NET Core: API, casos de uso, dominio y adaptadores de infraestructura MySQL, con interfaces cuando reduzcan dependencias concretas. Las relaciones y restricciones SQL deben asegurar integridad referencial y conservar historial de mantenimiento.

## 7. Seguridad y privacidad

La autenticación confirma identidad; la autorización determina los recursos permitidos. La interfaz nunca constituye una barrera de seguridad. Cada ruta privada valida sesión, rol y pertenencia del objeto, y debe existir prueba negativa sobre acceso a registros ajenos. Los expedientes y credenciales no se publican en GitHub.

## 8. Verificación y entrega

La calidad se demuestra con pruebas automatizadas, revisión de PR, documentación, criterios de aceptación y registro de decisiones. Un diseño propuesto o una captura de maqueta no prueba que el módulo esté operativo. Las revisiones por sprint registran incremento real y defectos pendientes.

## Referencias (APA 7)

Beck, K., & Cunningham, W. (1989). A laboratory for teaching object-oriented thinking. *ACM SIGPLAN Notices, 24*(10), 1–6. https://doi.org/10.1145/74878.74879

Coleman, J., & Vacanti, D. S. (2025). *The Kanban guide*. Kanban Guides. https://kanbanguides.org/the-kanban-guide

International Organization for Standardization. (2018). *Systems and software engineering—Life cycle processes—Requirements engineering (ISO/IEC/IEEE 29148:2018)*. https://www.iso.org/standard/72089.html

Microsoft. (s. f.). *Model-View-ViewModel*. Microsoft Learn. https://learn.microsoft.com/dotnet/architecture/maui/mvvm

OWASP Foundation. (s. f.). *Authorization cheat sheet*. OWASP Cheat Sheet Series. https://cheatsheetseries.owasp.org/cheatsheets/Authorization_Cheat_Sheet.html

Schwaber, K., & Sutherland, J. (2020). *The Scrum guide*. https://scrumguides.org/scrum-guide.html

World Wide Web Consortium. (2024). *Web Content Accessibility Guidelines (WCAG) 2.2*. https://www.w3.org/TR/WCAG22/
