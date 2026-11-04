# Seguridad de Follow Motors

El repositorio es público y debe contener **solo código y datos de prueba**.

- No incorporar datos personales de clientes, identificaciones, contraseñas, API keys ni archivos de configuración con secretos.
- El frontend no debe conectarse a MySQL. El acceso a datos es exclusivo del backend.
- Validar entradas y autorización en la API aunque ya se validen en el navegador.
- Cuando se implemente login, almacenar hashes de contraseñas con algoritmo apropiado y aplicar políticas de sesión y roles.
- En CI solo se usan credenciales efímeras de un MySQL de pruebas.
- Antes de producción: HTTPS, respaldos, mínimos privilegios, orígenes CORS autorizados, secretos en el entorno, pruebas de restauración y despliegue reversible.
- Informar hallazgos sensibles de forma privada a la administración del repositorio, sin publicarlos en Issues.
